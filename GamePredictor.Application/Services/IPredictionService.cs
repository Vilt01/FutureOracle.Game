using GamePredictor.Application.DTOs;

namespace GamePredictor.Application.Interfaces;

public interface IPredictionService
{
    Task<PredictionResultDto> PredictGameAsync(int gameId);
    Task<IEnumerable<GamePreviewDto>> GetUpcomingGamesPreviewAsync();
}