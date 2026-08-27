using System.Text.Json;
using GamePredictor.Application.Interfaces;
using GamePredictor.Infrastructure.Clients.DTOs;
using GamePredictor.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GamePredictor.Infrastructure.Clients;

public class YoutubeClient : IYoutubeClient
{
    private readonly HttpClient _httpClient;
    private readonly YoutubeOptions _options;
    private readonly ILogger<YoutubeClient> _logger;

    public YoutubeClient(HttpClient httpClient, IOptions<YoutubeOptions> options, ILogger<YoutubeClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<long> GetTrailerViewsAsync(string videoId)
    {
        if (string.IsNullOrEmpty(videoId))
            return 0;

        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/videos?part=statistics&id={videoId}&key={_options.ApiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube API вернул {StatusCode} для videoId {VideoId}", response.StatusCode, videoId);
                return 0;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YoutubeResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (result?.Items?.FirstOrDefault()?.Statistics?.ViewCount != null)
                return long.Parse(result.Items.First().Statistics.ViewCount);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения просмотров для videoId {VideoId}, возвращаем 0", videoId);
            return 0;
        }
    }

    // Поиск ID трейлера по названию игры через YouTube Search API
    public async Task<string?> FindTrailerIdAsync(string gameName)
    {
        try
        {
            var query = $"{gameName} official trailer";
            var url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&q={Uri.EscapeDataString(query)}&type=video&key={_options.ApiKey}&maxResults=1";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube Search вернул ошибку {StatusCode} для {GameName}", response.StatusCode, gameName);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YoutubeSearchResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var videoId = result?.Items?.FirstOrDefault()?.Id?.VideoId;
            if (!string.IsNullOrEmpty(videoId))
            {
                _logger.LogInformation("Найден YouTube videoId для {GameName}: {VideoId}", gameName, videoId);
                return videoId;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка поиска трейлера для {GameName}", gameName);
            return null;
        }
    }

    private class YoutubeSearchResponse
    {
        public List<YoutubeSearchItem> Items { get; set; } = new();
    }

    private class YoutubeSearchItem
    {
        public YoutubeSearchId Id { get; set; } = new();
    }

    private class YoutubeSearchId
    {
        public string VideoId { get; set; } = string.Empty;
    }
}