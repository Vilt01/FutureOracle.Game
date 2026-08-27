using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface IDeveloperRepository
{
    Task<Developers?> GetByIdAsync(int id);
    Task<Developers?> GetByNameAsync(string name);
    Task<Developers> GetOrCreateAsync(string name, decimal defaultAvg = 70m);
    Task UpdateAsync(Developers developer);
    Task<bool> SaveChangesAsync();
}