using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using GamePredictor.Domain.Entities;
using GamePredictor.Infrastructure.Clients.DTOs;
using GamePredictor.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GamePredictor.Infrastructure.Clients;

public class RawgClient : IGameSourceClient
{
    private readonly HttpClient _httpClient;
    private readonly RawgOptions _options;
    private readonly ILogger<RawgClient> _logger;

    public RawgClient(HttpClient httpClient, IOptions<RawgOptions> options, ILogger<RawgClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<Game>> GetUpcomingGamesAsync(DateTime fromDate, int daysAhead = 90)
    {
        try
        {
            var from = fromDate.ToString("yyyy-MM-dd");
            var to = fromDate.AddDays(daysAhead).ToString("yyyy-MM-dd");
            var url = $"https://api.rawg.io/api/games?key={_options.ApiKey}&dates={from},{to}&ordering=released&page_size=40";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RAWG вернул ошибку: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<Game>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RawgResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Results == null)
                return Enumerable.Empty<Game>();

            var games = result.Results.Select(dto => new Game
            {
                RawgId = dto.Id,
                Title = dto.Name ?? "Unknown",
                Genre = dto.Genres?.FirstOrDefault()?.Name ?? "Unknown",
                Releasedate = DateOnly.TryParse(dto.Released, out var date) ? date : null,
                Platforms = dto.Platforms != null ? string.Join(", ", dto.Platforms.Select(p => p.Platform?.Name ?? "Unknown")) : string.Empty,
                IsReleased = false,
                MetacriticScore = dto.Metacritic,
                SteamAppId = null,
                TrailerYoutubeId = null,
                DeveloperId = 1 // временно, будет заменено
            }).ToList();

            _logger.LogInformation("Получено {Count} игр из RAWG", games.Count);
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе к RAWG");
            return Enumerable.Empty<Game>();
        }
    }

    public async Task<GameDetailsDto?> GetGameDetailsAsync(int rawgId)
    {
        try
        {
            var url = $"https://api.rawg.io/api/games/{rawgId}?key={_options.ApiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RAWG details вернул ошибку: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<RawgGameDetailsDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto == null) return null;

            // Обработка разработчиков: если пусто – добавляем Unknown
            var developers = dto.Developers?.Select(d => new DeveloperInfo { Id = d.Id, Name = d.Name }).ToList();
            if (developers == null || developers.Count == 0)
            {
                developers = new List<DeveloperInfo> { new DeveloperInfo { Id = 0, Name = "Unknown" } };
                _logger.LogWarning("Для игры {Name} (ID {RawgId}) не найдены разработчики, добавлен Unknown", dto.Name, rawgId);
            }

            return new GameDetailsDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Developers = developers,
                Stores = dto.Stores?.Select(s => new StoreInfo
                {
                    Store = s.Store != null ? new StoreDetail { Id = s.Store.Id, Name = s.Store.Name } : null,
                    Url = s.Url ?? ""
                }).ToList() ?? new(),
                Clip = dto.Clip != null ? new ClipInfo { Url = dto.Clip.Url } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения деталей игры {RawgId}", rawgId);
            return null;
        }
    }

    public async Task<IEnumerable<DeveloperGameStatsDto>> GetGamesForDeveloperAsync(string developerName, int limit = 3)
    {
        try
        {
            var devId = await GetDeveloperIdByNameAsync(developerName);
            if (!devId.HasValue)
            {
                _logger.LogWarning("Не найден разработчик с именем {DeveloperName}", developerName);
                return Enumerable.Empty<DeveloperGameStatsDto>();
            }

            var url = $"https://api.rawg.io/api/games?key={_options.ApiKey}&developers={devId.Value}&ordering=-released&page_size={limit}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RAWG (games for developer) вернул ошибку: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<DeveloperGameStatsDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DeveloperGamesResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Results?.Select(dto => new DeveloperGameStatsDto
            {
                MetacriticScore = dto.Metacritic,
                ReleaseDate = DateOnly.TryParse(dto.Released, out var date) ? date : null
            }) ?? Enumerable.Empty<DeveloperGameStatsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении игр разработчика {DeveloperName}", developerName);
            return Enumerable.Empty<DeveloperGameStatsDto>();
        }
    }

    public async Task<int?> GetDeveloperIdByNameAsync(string name)
    {
        try
        {
            var encodedName = Uri.EscapeDataString(name);
            var url = $"https://api.rawg.io/api/developers?key={_options.ApiKey}&search={encodedName}&page_size=1";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DeveloperSearchResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Results?.FirstOrDefault()?.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка поиска разработчика {Name}", name);
            return null;
        }
    }

    // Внутренние классы для десериализации
    private class RawgResponse
    {
        public List<RawgGameDto> Results { get; set; } = new();
    }

    private class RawgGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Released { get; set; } = string.Empty;
        public int? Metacritic { get; set; }
        public List<RawgGenreDto>? Genres { get; set; }
        public List<RawgPlatformDto>? Platforms { get; set; }
        public List<RawgStoreDto>? Stores { get; set; }
        public RawgClipDto? Clip { get; set; }
        public List<RawgDeveloperDto>? Developers { get; set; }
    }

    private class RawgGenreDto { public string Name { get; set; } = string.Empty; }
    private class RawgPlatformDto { public RawgPlatformInfo? Platform { get; set; } }
    private class RawgPlatformInfo { public string Name { get; set; } = string.Empty; }
    private class RawgStoreDto { public RawgStoreInfo? Store { get; set; } public string Url { get; set; } = string.Empty; }
    private class RawgStoreInfo { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    private class RawgClipDto { public string Url { get; set; } = string.Empty; }
    private class RawgDeveloperDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    private class DeveloperGamesResponse { public List<DeveloperGameDto> Results { get; set; } = new(); }
    private class DeveloperGameDto { public int? Metacritic { get; set; } public string Released { get; set; } = string.Empty; }
    private class DeveloperSearchResponse { public List<DeveloperSearchResult> Results { get; set; } = new(); }
    private class DeveloperSearchResult { public int Id { get; set; } }
}