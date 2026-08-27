using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface INewsRepository
{
    Task<IEnumerable<NewsSentiment>> GetForGameAsync(int gameId, int days);
    void AddRange(IEnumerable<NewsSentiment> news);
    Task<bool> SaveChangesAsync();
}