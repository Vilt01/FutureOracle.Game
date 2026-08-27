using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface IMetricRepository
{
    Task<PreReleaseMetrics?> GetLatestForGameAsync(int gameId);
    void Add(PreReleaseMetrics metric);
    Task<bool> SaveChangesAsync();

    // НОВЫЙ МЕТОД (добавьте в конец)
    Task<double> GetMedianWishlistForGenreAsync(string genre);
}