using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GamePredictor.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IPredictionService _predictionService;
    private readonly IDataUpdateService _dataUpdateService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IPredictionService predictionService,
        IDataUpdateService dataUpdateService,
        ILogger<IndexModel> logger)
    {
        _predictionService = predictionService;
        _dataUpdateService = dataUpdateService;
        _logger = logger;
    }

    public IEnumerable<GamePreviewDto> Games { get; set; } = new List<GamePreviewDto>();

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Загрузка главной страницы");
        Games = await _predictionService.GetUpcomingGamesPreviewAsync();
        _logger.LogInformation("Найдено {Count} игр", Games.Count());
        foreach (var game in Games)
        {
            _logger.LogInformation("Игра: {Title}, Id={Id}", game.Title, game.Id);
        }
    }

    public async Task<IActionResult> OnPost()
    {
        _logger.LogInformation("=== OnPost вызван ===");
        try
        {
            var result = await _dataUpdateService.UpdateAllDataAsync();
            _logger.LogInformation("Обновление завершено. Игр: {Games}, прогнозов: {Predictions}",
                result.GamesLoaded, result.PredictionsCalculated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении");
        }
        return RedirectToPage();
    }
}