using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Repositories;

public class GameRepository : RepositoryBase<Game>, IGameRepository
{
    public GameRepository(AppDbContext context) : base(context) { }

    public override async Task<Game?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Developer)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetUpcomingAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Include(g => g.Developer)
            .Where(g => !g.IsReleased && g.Releasedate >= DateOnly.FromDateTime(from) && g.Releasedate <= DateOnly.FromDateTime(to))
            .OrderBy(g => g.Releasedate)
            .ToListAsync();
    }

    public async Task<double> GetGenreAverageAsync(string genre)
    {
        var twoYearsAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));
        var avg = await _context.Games
            .Where(g => g.Genre == genre && g.IsReleased && g.MetacriticScore.HasValue && g.Releasedate >= twoYearsAgo)
            .AverageAsync(g => (double?)g.MetacriticScore.Value);
        return avg ?? 75.0;
    }

    public async Task<Game?> GetByRawgIdAsync(int rawgId)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.RawgId == rawgId);
    }

    public async Task<Developers> GetOrCreateDeveloperAsync(string name)
    {
        var dev = await _context.Developers.FirstOrDefaultAsync(d => d.Name == name);
        if (dev == null)
        {
            dev = new Developers
            {
                Name = name,
                AvgMetacriticLast3 = 70,
                GamesCount = 0
            };
            _context.Developers.Add(dev);
            await _context.SaveChangesAsync();
        }
        return dev;
    }

    public async Task<Developers?> GetDeveloperByIdAsync(int id)
    {
        return await _context.Developers.FindAsync(id);
    }
}
