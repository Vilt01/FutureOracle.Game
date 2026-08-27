// GamePredictor.Application/Services/PredictionService.cs
using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GamePredictor.Application.Options;

namespace GamePredictor.Application.Services;

public class PredictionService : IPredictionService
{
    private readonly IGameRepository _gameRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IMetricRepository _metricRepository;
    private readonly INewsRepository _newsRepository;
    private readonly IGenreStatsService _genreStatsService; // новый сервис
    private readonly PredictionOptions _predictionOptions;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(
        IGameRepository gameRepository,
        IPredictionRepository predictionRepository,
        IMetricRepository metricRepository,
        INewsRepository newsRepository,
        IGenreStatsService genreStatsService, // добавлен
        IOptions<PredictionOptions> options,
        ILogger<PredictionService> logger)
    {
        _gameRepository = gameRepository;
        _predictionRepository = predictionRepository;
        _metricRepository = metricRepository;
        _newsRepository = newsRepository;
        _genreStatsService = genreStatsService;
        _predictionOptions = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<GamePreviewDto>> GetUpcomingGamesPreviewAsync()
    {
        var games = await _gameRepository.GetUpcomingAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(90));
        var result = new List<GamePreviewDto>();
        foreach (var game in games)
        {
            var latestPrediction = await _predictionRepository.GetLatestForGameAsync(game.Id);
            result.Add(new GamePreviewDto
            {
                Id = game.Id,
                Title = game.Title,
                DeveloperName = game.Developer?.Name ?? "Unknown",
                ReleaseDate = game.Releasedate,
                PredictedScore = latestPrediction != null ? (double)latestPrediction.PredictedMetacritic : null,
                Confidence = latestPrediction != null ? (double)latestPrediction.Confidence : null
            });
        }
        return result;
    }

    public async Task<PredictionResultDto> PredictGameAsync(int gameId)
    {
        _logger.LogInformation("Прогноз для игры {GameId}", gameId);

        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new Exception($"Игра с ID {gameId} не найдена");

        var metrics = await _metricRepository.GetLatestForGameAsync(gameId);
        var news = await _newsRepository.GetForGameAsync(gameId, 60);

        // 1. Базовый рейтинг
        double studioAvg = (double)(game.Developer?.AvgMetacriticLast3 ?? 70m);
        // Используем сервис статистики жанров
        double genreAvg = await _genreStatsService.GetAverageScoreForGenreAsync(game.Genre);
        double studioWeight = _predictionOptions.StudioWeight;
        double genreWeight = _predictionOptions.GenreWeight;
        double baseScore = studioWeight * studioAvg + genreWeight * genreAvg;

        // 2. Коррекция по wishlist
        double interestFactor = 1.0;
        bool hasWishlist = metrics != null && metrics.WishlistCount > 0;
        if (hasWishlist)
        {
            double medianWishlist = await _metricRepository.GetMedianWishlistForGenreAsync(game.Genre);
            if (medianWishlist > 0)
                interestFactor = Math.Clamp((double)metrics.WishlistCount / medianWishlist, 0.5, 2.0);
        }
        double baseScoreAfterInterest = hasWishlist ? baseScore * (0.9 + 0.2 * interestFactor) : baseScore;

        // 3. Бонус/штраф за просмотры
        double trailerBonus = 0;
        bool hasViews = metrics != null && metrics.YoutubeTrailerViews > 0;
        if (hasViews)
        {
            if (metrics.YoutubeTrailerViews > _predictionOptions.HighViewsThreshold)
                trailerBonus = 3;
            else if (metrics.YoutubeTrailerViews < _predictionOptions.LowViewsThreshold)
                trailerBonus = -3;
        }
        double scoreAfterTrailer = Math.Clamp(baseScoreAfterInterest + trailerBonus, 0, 100);

        // 4. Сентимент новостей
        double sentimentDelta = 0;
        bool hasNews = news.Any();
        if (hasNews)
        {
            var validNews = news.Where(n => n.SentimentScore.HasValue && n.Relevance.HasValue);
            if (validNews.Any())
            {
                double avgSentiment = validNews.Average(n => (double)(n.SentimentScore.Value * n.Relevance.Value));
                sentimentDelta = Math.Clamp(avgSentiment * 10, -10, 10);
            }
        }
        double finalScore = Math.Clamp(scoreAfterTrailer + sentimentDelta, 0, 100);

        // 5. Класс продаж
        string salesClass = "Niche";
        if (hasWishlist && hasNews)
        {
            if (metrics.WishlistCount > _predictionOptions.BlockbusterWishlistThreshold && sentimentDelta > 3)
                salesClass = "Blockbuster";
            else if (metrics.WishlistCount >= _predictionOptions.AverageWishlistThreshold && sentimentDelta > -1)
                salesClass = "Average";
        }
        else if (hasWishlist && metrics.WishlistCount > _predictionOptions.BlockbusterWishlistThreshold)
            salesClass = "Average";

        // 6. Уверенность
        double dataCompleteness = 0.2;
        if (hasWishlist) dataCompleteness += 0.3;
        if (hasViews) dataCompleteness += 0.25;
        if (hasNews) dataCompleteness += 0.25;
        dataCompleteness = Math.Min(dataCompleteness, 1.0);

        var components = new[] { baseScore, baseScoreAfterInterest, finalScore };
        double mean = components.Average();
        double stdDev = Math.Sqrt(components.Select(x => Math.Pow(x - mean, 2)).Average());
        double normalizedStdDev = Math.Min(stdDev / 100, 1.0);
        double confidence = (1 - normalizedStdDev) * dataCompleteness;
        confidence = Math.Clamp(confidence, 0.05, 0.99);

        // 7. Риск
        string riskLevel = "High";
        if (dataCompleteness > 0.6 && confidence > 0.5)
            riskLevel = "Medium";
        if (dataCompleteness > 0.8 && confidence > 0.7)
            riskLevel = "Low";

        // 8. Аргументы
        string arguments = $"Студия: {studioAvg:F1}, жанр: {genreAvg:F1} → база {baseScore:F1}. ";
        if (hasWishlist)
            arguments += $"Wishlist: {metrics.WishlistCount} → коэф. {interestFactor:F2} → {baseScoreAfterInterest:F1}. ";
        else
            arguments += "Wishlist: нет данных. ";
        if (hasViews)
            arguments += $"Просмотры: {metrics.YoutubeTrailerViews} → бонус {trailerBonus:F0}. ";
        else
            arguments += "Просмотры: нет данных. ";
        if (hasNews)
            arguments += $"Сентимент: {news.Average(n => (double?)n.SentimentScore)?.ToString("F2")} → поправка {sentimentDelta:F1}. ";
        else
            arguments += "Новости: нет данных. ";
        arguments += $"Итог: {finalScore:F1}.";

        var prediction = new Predictions
        {
            GameId = gameId,
            PredictedMetacritic = (decimal)finalScore,
            SalesClass = salesClass,
            Confidence = (decimal)confidence,
            RiskLevel = riskLevel,
            Arguments = arguments,
            CreatedAt = DateTime.UtcNow,
            Verified = null
        };

        _predictionRepository.Add(prediction);
        await _predictionRepository.SaveChangesAsync();

        _logger.LogInformation("Прогноз для игры {GameId}: {Score}, уверенность {Confidence:P0}, данных: {DataCompleteness:P0}", gameId, finalScore, confidence, dataCompleteness);

        return new PredictionResultDto
        {
            GameId = gameId,
            GameTitle = game.Title,
            PredictedScore = finalScore,
            SalesClass = salesClass,
            Confidence = confidence,
            RiskLevel = riskLevel,
            Arguments = arguments,
            CreatedAt = prediction.CreatedAt
        };
    }
}