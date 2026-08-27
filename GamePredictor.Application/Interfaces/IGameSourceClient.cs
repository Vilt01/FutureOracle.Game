using GamePredictor.Domain.Entities;
using GamePredictor.Application.DTOs;

namespace GamePredictor.Application.Interfaces;

public interface IGameSourceClient
{
    Task<IEnumerable<Game>> GetUpcomingGamesAsync(DateTime fromDate, int daysAhead = 90);
    Task<GameDetailsDto?> GetGameDetailsAsync(int rawgId);
    Task<IEnumerable<DeveloperGameStatsDto>> GetGamesForDeveloperAsync(string developerName, int limit = 3); // новый метод
}

// DTO для деталей игры
public class GameDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<DeveloperInfo> Developers { get; set; } = new();
    public List<StoreInfo> Stores { get; set; } = new();
    public ClipInfo? Clip { get; set; } // 👈 добавлено свойство Clip
}

public class DeveloperInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StoreInfo
{
    public StoreDetail? Store { get; set; } // 👈 свойство Store
    public string Url { get; set; } = string.Empty;
}

public class StoreDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ClipInfo
{
    public string Url { get; set; } = string.Empty;
}