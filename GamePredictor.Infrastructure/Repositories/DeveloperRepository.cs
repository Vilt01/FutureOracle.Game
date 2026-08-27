using GamePredictor.Domain.Entities;
using GamePredictor.Domain.Interfaces;
using GamePredictor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GamePredictor.Infrastructure.Repositories;

public class DeveloperRepository : IDeveloperRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<Developers> _dbSet;

    public DeveloperRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Developers>();
    }

    public async Task<Developers?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Developers?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Name == name);
    }

    public async Task<Developers> GetOrCreateAsync(string name, decimal defaultAvg = 70m)
    {
        var dev = await GetByNameAsync(name);
        if (dev == null)
        {
            dev = new Developers
            {
                Name = name,
                AvgMetacriticLast3 = defaultAvg,
                GamesCount = 0
            };
            _dbSet.Add(dev);
        }
        return dev;
    }

    public async Task UpdateAsync(Developers developer)
    {
        _dbSet.Update(developer);
        // ⚠️ НЕ ВЫЗЫВАЕМ SaveChangesAsync здесь – это делает вызывающий код
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}