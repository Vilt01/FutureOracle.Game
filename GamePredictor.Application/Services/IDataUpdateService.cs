using GamePredictor.Domain.Entities;

namespace GamePredictor.Application.Interfaces;

public interface IDataUpdateService
{
    Task<(int GamesLoaded, int PredictionsCalculated)> UpdateAllDataAsync();
}