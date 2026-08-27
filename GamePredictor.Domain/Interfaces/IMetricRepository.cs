using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface IMetricRepository
{
    Task<PreReleaseMetrics?> GetLatestForGameAsync(int gameId);
    void Add(PreReleaseMetrics metric);
    Task<bool> SaveChangesAsync();

    Task<double> GetMedianWishlistForGenreAsync(string genre);
}
