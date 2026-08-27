using GamePredictor.Application.Interfaces;
using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace GamePredictor.Infrastructure.Services;

public class DataUpdateService : IDataUpdateService
{
    private readonly IGameRepository _gameRepository;
    private readonly IDeveloperRepository _developerRepository;
    private readonly IPredictionService _predictionService;
    private readonly IGameSourceClient _gameSourceClient;
    private readonly INewsApiClient _newsApiClient;
    private readonly ISteamClient _steamClient;
    private readonly IYoutubeClient _youtubeClient;
    private readonly ISentimentClient _sentimentClient;
    private readonly IMetricRepository _metricRepository;
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<DataUpdateService> _logger;

    public DataUpdateService(
        IGameRepository gameRepository,
        IDeveloperRepository developerRepository,
        IPredictionService predictionService,
        IGameSourceClient gameSourceClient,
        INewsApiClient newsApiClient,
        ISteamClient steamClient,
        IYoutubeClient youtubeClient,
        ISentimentClient sentimentClient,
        IMetricRepository metricRepository,
        INewsRepository newsRepository,
        ILogger<DataUpdateService> logger)
    {
        _gameRepository = gameRepository;
        _developerRepository = developerRepository;
        _predictionService = predictionService;
        _gameSourceClient = gameSourceClient;
        _newsApiClient = newsApiClient;
        _steamClient = steamClient;
        _youtubeClient = youtubeClient;
        _sentimentClient = sentimentClient;
        _metricRepository = metricRepository;
        _newsRepository = newsRepository;
        _logger = logger;
    }

    public async Task<(int GamesLoaded, int PredictionsCalculated)> UpdateAllDataAsync()
    {
        _logger.LogInformation("Начинаем обновление данных...");
        int gamesLoaded = 0, predictionsCount = 0;
        var developersToUpdate = new List<(int Id, string Name)>();

        var games = await _gameSourceClient.GetUpcomingGamesAsync(DateTime.UtcNow, 90);
        gamesLoaded = games.Count();
        _logger.LogInformation("Получено {Count} игр из RAWG", gamesLoaded);

        foreach (var game in games)
        {
            if (!game.RawgId.HasValue) continue;

            var details = await _gameSourceClient.GetGameDetailsAsync(game.RawgId.Value);
            if (details == null) continue;

            if (details.Developers != null && details.Developers.Any())
            {
                var devName = details.Developers.First().Name;
                if (!string.IsNullOrEmpty(devName))
                {
                    var dev = await _developerRepository.GetOrCreateAsync(devName);
                    game.DeveloperId = dev.Id;
                    if (!developersToUpdate.Any(d => d.Id == dev.Id))
                        developersToUpdate.Add((dev.Id, devName));
                }
            }
            else
            {
                var unknownDev = await _developerRepository.GetOrCreateAsync("Unknown", 70m);
                game.DeveloperId = unknownDev.Id;
            }

            if (details.Stores != null)
            {
                foreach (var storeInfo in details.Stores)
                {
                    if (storeInfo.Store?.Name?.Contains("Steam", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var match = Regex.Match(storeInfo.Url, @"(?:app|steampowered\.com/app)/(\d+)");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int appId))
                        {
                            game.SteamAppId = appId;
                            _logger.LogInformation("SteamAppId для {Title}: {AppId} (из RAWG)", game.Title, appId);
                            break;
                        }
                    }
                }
            }

            if (!game.SteamAppId.HasValue)
            {
                var foundAppId = await _steamClient.FindAppIdByNameAsync(game.Title);
                if (foundAppId.HasValue)
                {
                    game.SteamAppId = foundAppId.Value;
                    _logger.LogInformation("SteamAppId для {Title}: {AppId} (найден по названию)", game.Title, foundAppId.Value);
                }
            }

            if (details.Clip != null && !string.IsNullOrEmpty(details.Clip.Url))
            {
                var vidMatch = Regex.Match(details.Clip.Url, @"(?:v=|youtu\.be/)([^&?]+)");
                if (vidMatch.Success)
                {
                    game.TrailerYoutubeId = vidMatch.Groups[1].Value;
                    _logger.LogInformation("YouTube videoId для {Title}: {VideoId} (из RAWG)", game.Title, game.TrailerYoutubeId);
                }
            }

            if (string.IsNullOrEmpty(game.TrailerYoutubeId))
            {
                var foundVideoId = await _youtubeClient.FindTrailerIdAsync(game.Title);
                if (!string.IsNullOrEmpty(foundVideoId))
                {
                    game.TrailerYoutubeId = foundVideoId;
                    _logger.LogInformation("YouTube videoId для {Title}: {VideoId} (найден по названию)", game.Title, foundVideoId);
                }
            }

            var existing = await _gameRepository.GetByRawgIdAsync(game.RawgId.Value);
            if (existing == null)
            {
                _gameRepository.Add(game);
            }
            else
            {
                existing.Title = game.Title;
                existing.Genre = game.Genre;
                existing.Releasedate = game.Releasedate;
                existing.Platforms = game.Platforms;
                existing.DeveloperId = game.DeveloperId;
                existing.BudgetEstimate = game.BudgetEstimate;
                existing.MetacriticScore = game.MetacriticScore;
                existing.IsReleased = game.IsReleased;

                if (game.SteamAppId.HasValue)
                    existing.SteamAppId = game.SteamAppId.Value;

                if (!string.IsNullOrEmpty(game.TrailerYoutubeId))
                    existing.TrailerYoutubeId = game.TrailerYoutubeId;

                _gameRepository.Update(existing);
            }
        }

        await _gameRepository.SaveChangesAsync();

        foreach (var (devId, devName) in developersToUpdate)
        {
            await UpdateDeveloperAverageAsync(devId, devName);
        }

        var savedGames = await _gameRepository.GetUpcomingAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(90));
        _logger.LogInformation("Обработка {Count} сохранённых игр", savedGames.Count());

        foreach (var game in savedGames)
        {
            await UpdateMetricsForGameAsync(game);
            await UpdateNewsForGameAsync(game);

            try
            {
                await _predictionService.PredictGameAsync(game.Id);
                predictionsCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка прогноза для игры {Title}", game.Title);
            }
        }

        _logger.LogInformation("Обновление завершено. Загружено: {GamesLoaded}, прогнозов: {PredictionsCalculated}", gamesLoaded, predictionsCount);
        return (gamesLoaded, predictionsCount);
    }

    private async Task UpdateMetricsForGameAsync(Game game)
    {
        int wishlist = 0;
        long? views = null;
        bool gotWishlist = false;
        bool gotViews = false;

        if (game.SteamAppId.HasValue && game.SteamAppId.Value > 0)
        {
            try
            {
                wishlist = await _steamClient.GetWishlistCountAsync(game.SteamAppId.Value);
                gotWishlist = true;
                _logger.LogInformation("Wishlist для {Title}: {Wishlist}", game.Title, wishlist);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка wishlist для {Title}", game.Title);
            }
        }

        if (!string.IsNullOrEmpty(game.TrailerYoutubeId))
        {
            try
            {
                views = await _youtubeClient.GetTrailerViewsAsync(game.TrailerYoutubeId);
                gotViews = true;
                _logger.LogInformation("Просмотры для {Title}: {Views}", game.Title, views);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка просмотров для {Title}", game.Title);
            }
        }

        // Проверяем, есть ли уже хорошая запись
        var previousMetric = await _metricRepository.GetLatestForGameAsync(game.Id);
        bool hasPreviousNonZero = previousMetric != null &&
                                  (previousMetric.WishlistCount > 0 || previousMetric.YoutubeTrailerViews > 0);

        if ((gotWishlist && wishlist > 0) || (gotViews && views > 0) || !hasPreviousNonZero)
        {
            var metric = new PreReleaseMetrics
            {
                GameId = game.Id,
                Timestamp = DateTime.UtcNow,
                WishlistCount = wishlist,
                YoutubeTrailerViews = views ?? 0,
                RedditMentions = 0,
                TwitchViewerAvg = null
            };
            _metricRepository.Add(metric);
            await _metricRepository.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("Для игры {Title} новые данные не получены, старая метрика сохранена", game.Title);
        }
    }

    private async Task UpdateNewsForGameAsync(Game game)
    {
        var existingNews = await _newsRepository.GetForGameAsync(game.Id, 3);
        if (existingNews.Any())
        {
            _logger.LogInformation("Для игры {Title} уже есть свежие новости, пропускаем", game.Title);
            return;
        }

        try
        {
            var articles = await _newsApiClient.GetNewsForGameAsync(game.Title, 60);
            if (!articles.Any()) return;

            foreach (var article in articles)
            {
                var textForSentiment = !string.IsNullOrEmpty(article.Description) ? article.Description : article.Title;
                double sentimentScore = 0;
                try
                {
                    sentimentScore = await _sentimentClient.GetSentimentAsync(textForSentiment);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка сентимент-анализа для {Title}", article.Title);
                }
                var newsSentiment = new NewsSentiment
                {
                    GameId = game.Id,
                    Source = article.Source.Name ?? "Unknown",
                    PublishedAt = article.PublishedAt,
                    SentimentScore = (decimal)sentimentScore,
                    Relevance = article.Title.Contains(game.Title, StringComparison.OrdinalIgnoreCase) ? 1.0m : 0.6m,
                    Keywords = string.Empty
                };
                _newsRepository.AddRange(new List<NewsSentiment> { newsSentiment });
            }
            await _newsRepository.SaveChangesAsync();
            _logger.LogInformation("Новости для {Title} сохранены", game.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки новостей для игры {Title}", game.Title);
        }
    }

    private async Task UpdateDeveloperAverageAsync(int developerId, string developerName)
    {
        try
        {
            await Task.Delay(500);
            var games = await _gameSourceClient.GetGamesForDeveloperAsync(developerName, 3);
            if (!games.Any()) return;
            var validGames = games.Where(g => g.MetacriticScore.HasValue && g.MetacriticScore.Value > 0).ToList();
            var avg = validGames.Any() ? validGames.Average(g => g.MetacriticScore.Value) : 70.0;
            var developer = await _developerRepository.GetByIdAsync(developerId);
            if (developer != null)
            {
                developer.AvgMetacriticLast3 = (decimal)avg;
                await _developerRepository.UpdateAsync(developer);
                await _developerRepository.SaveChangesAsync();
                _logger.LogInformation("Средний балл для {DeveloperName}: {Avg:F1} (на основе {Count} игр)", developerName, avg, games.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обновления среднего балла для {DeveloperName}", developerName);
        }
    }
}
