using System.Text.Json;
using System.Text.RegularExpressions;
using GamePredictor.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GamePredictor.Infrastructure.Clients;

public class SteamClient : ISteamClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SteamClient> _logger;

    public SteamClient(HttpClient httpClient, ILogger<SteamClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<int> GetWishlistCountAsync(int appId)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var url = $"https://steamspy.com/api.php?request=appdetails&appid={appId}";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var response = await _httpClient.GetStringAsync(url, cts.Token);
                var data = JsonSerializer.Deserialize<SteamSpyResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null && data.Wishlist > 0)
                {
                    _logger.LogInformation("Wishlist для appId {AppId}: {Wishlist} (Steam Spy)", appId, data.Wishlist);
                    return data.Wishlist;
                }
                break; 
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Попытка {Attempt} Steam Spy для appId {AppId} не удалась", attempt, appId);
                if (attempt == 3) break;
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2));
            }
        }

        try
        {
            var url = $"https://steamdb.info/app/{appId}/";
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var html = await _httpClient.GetStringAsync(url, cts.Token);
            var match = Regex.Match(html, @"<span[^>]*class=""[^""]*number[^""]*""[^>]*>([\d,]+)</span>");
            if (match.Success)
            {
                var raw = match.Groups[1].Value.Replace(",", "");
                if (int.TryParse(raw, out var wishlist))
                {
                    _logger.LogInformation("Wishlist для appId {AppId}: {Wishlist} (SteamDB)", appId, wishlist);
                    return wishlist;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка парсинга SteamDB для appId {AppId}", appId);
        }

        _logger.LogWarning("Не удалось получить wishlist для appId {AppId}, возвращаем 0", appId);
        return 0;
    }

    public async Task<int?> FindAppIdByNameAsync(string gameName)
    {
        try
        {
            var url = $"https://store.steampowered.com/api/storesearch?term={Uri.EscapeDataString(gameName)}&l=english&cc=US";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<SteamSearchResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var firstItem = result?.Items?.FirstOrDefault();
            if (firstItem != null && firstItem.Id > 0)
            {
                _logger.LogInformation("Найден SteamAppId для {GameName}: {AppId}", gameName, firstItem.Id);
                return firstItem.Id;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка поиска SteamAppId для {GameName}", gameName);
            return null;
        }
    }

    private class SteamSpyResponse
    {
        public int Wishlist { get; set; }
    }

    private class SteamSearchResponse
    {
        public List<SteamSearchItem> Items { get; set; } = new();
    }

    private class SteamSearchItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
