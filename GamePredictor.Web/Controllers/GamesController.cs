using GamePredictor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamePredictor.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public GamesController(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUpcoming()
    {
        var games = await _predictionService.GetUpcomingGamesPreviewAsync();
        return Ok(games);
    }

    [HttpGet("{id}/predict")]
    public async Task<IActionResult> Predict(int id)
    {
        try
        {
            var result = await _predictionService.PredictGameAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}