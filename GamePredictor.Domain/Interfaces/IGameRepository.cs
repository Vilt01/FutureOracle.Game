using GamePredictor.Domain.Entities;

namespace GamePredictor.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(int id);
    Task<IEnumerable<Game>> GetUpcomingAsync(DateTime from, DateTime to);
    void Add(Game game);
    void Update(Game game);
    Task<bool> SaveChangesAsync();
    Task<double> GetGenreAverageAsync(string genre);
    Task<Game?> GetByRawgIdAsync(int rawgId);
    Task<Developers> GetOrCreateDeveloperAsync(string name);
    Task<Developers?> GetDeveloperByIdAsync(int id);
}
