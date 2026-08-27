using Microsoft.AspNetCore.Mvc;
using GamePredictor.Application.Interfaces;

namespace GamePredictor.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDataUpdateService _dataUpdateService;

    public AdminController(IDataUpdateService dataUpdateService)
    {
        _dataUpdateService = dataUpdateService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateData()
    {
        try
        {
            var result = await _dataUpdateService.UpdateAllDataAsync();
            return Ok(new { message = $"Обновление завершено. Загружено игр: {result.GamesLoaded}, прогнозов: {result.PredictionsCalculated}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}