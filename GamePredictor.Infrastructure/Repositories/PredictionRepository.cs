using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Repositories;

public class PredictionRepository : RepositoryBase<Predictions>, IPredictionRepository
{
    public PredictionRepository(AppDbContext context) : base(context) { }

    public async Task<Predictions?> GetLatestForGameAsync(int gameId)
    {
        return await _dbSet
            .Where(p => p.GameId == gameId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }
}