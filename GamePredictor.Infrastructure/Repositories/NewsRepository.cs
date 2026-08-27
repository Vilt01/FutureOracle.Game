using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Repositories;

public class NewsRepository : RepositoryBase<NewsSentiment>, INewsRepository
{
    public NewsRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<NewsSentiment>> GetForGameAsync(int gameId, int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Where(n => n.GameId == gameId && n.PublishedAt >= cutoff)
            .OrderByDescending(n => n.PublishedAt)
            .ToListAsync();
    }

    public void AddRange(IEnumerable<NewsSentiment> news)
    {
        _dbSet.AddRange(news);
    }
}