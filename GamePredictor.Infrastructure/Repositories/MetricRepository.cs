using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Repositories;

public class MetricRepository : RepositoryBase<PreReleaseMetrics>, IMetricRepository
{
    public MetricRepository(AppDbContext context) : base(context) { }

    public async Task<PreReleaseMetrics?> GetLatestForGameAsync(int gameId)
    {
        return await _dbSet
            .Where(m => m.GameId == gameId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }

    // Реализация нового метода
    public async Task<double> GetMedianWishlistForGenreAsync(string genre)
    {
        var metrics = await _context.PreReleaseMetrics
            .Include(m => m.Game)
            .Where(m => m.Game.Genre == genre && m.WishlistCount > 0)
            .OrderBy(m => m.WishlistCount)
            .ToListAsync();

        if (!metrics.Any())
            return 50000.0; // значение по умолчанию

        int count = metrics.Count;
        if (count % 2 == 1)
            return metrics[count / 2].WishlistCount;
        else
            return (metrics[count / 2 - 1].WishlistCount + metrics[count / 2].WishlistCount) / 2.0;
    }
}