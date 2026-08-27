using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using GamePredictor.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GamePredictor.Web.Pages;

public class DetailsModel : PageModel
{
    private readonly IPredictionService _predictionService;
    private readonly IGameRepository _gameRepository;
    private readonly IMetricRepository _metricRepository;
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        IPredictionService predictionService,
        IGameRepository gameRepository,
        IMetricRepository metricRepository,
        INewsRepository newsRepository,
        ILogger<DetailsModel> logger)
    {
        _predictionService = predictionService;
        _gameRepository = gameRepository;
        _metricRepository = metricRepository;
        _newsRepository = newsRepository;
        _logger = logger;
    }

    public PredictionResultDto? Prediction { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public int? WishlistCount { get; set; }
    public long? YoutubeViews { get; set; }
    public int NewsCount { get; set; }
    public double? AvgSentiment { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        _logger.LogInformation("Вызван OnGetAsync с id = {Id}", id);

        try
        {
            Prediction = await _predictionService.PredictGameAsync(id);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Не удалось получить прогноз: {ex.Message}";
            _logger.LogError(ex, "Ошибка при получении прогноза для id {Id}", id);
        }

        var game = await _gameRepository.GetByIdAsync(id);
        if (game == null)
        {
            _logger.LogWarning("Игра с id {Id} не найдена", id);
            return RedirectToPage("./Error");
        }

        GameTitle = game.Title;
        DeveloperName = game.Developer?.Name ?? "Unknown";
        Genre = game.Genre;
        ReleaseDate = game.Releasedate;

        var metrics = await _metricRepository.GetLatestForGameAsync(id);
        WishlistCount = metrics?.WishlistCount;
        YoutubeViews = metrics?.YoutubeTrailerViews;

        var news = await _newsRepository.GetForGameAsync(id, 60);
        NewsCount = news.Count();
        AvgSentiment = news.Any()
            ? (double)news.Average(n => (double)(n.SentimentScore ?? 0))
            : null;

        return Page();
    }
}
