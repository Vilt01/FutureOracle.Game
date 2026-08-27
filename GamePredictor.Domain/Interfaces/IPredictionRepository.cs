using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface IPredictionRepository
{
    Task<Predictions?> GetLatestForGameAsync(int gameId);
    void Add(Predictions prediction);
    Task<bool> SaveChangesAsync();
}