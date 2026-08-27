using System.Text.Json;
using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using GamePredictor.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GamePredictor.Infrastructure.Clients;

public class NewsApiClient : INewsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly NewsApiOptions _options;
    private readonly ILogger<NewsApiClient> _logger;

    public NewsApiClient(HttpClient httpClient, IOptions<NewsApiOptions> options, ILogger<NewsApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsArticleDto>> GetNewsForGameAsync(string gameTitle, int daysBack = 60)
    {
        var from = DateTime.UtcNow.AddDays(-daysBack).ToString("yyyy-MM-dd");
        var url = $"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(gameTitle + " game")}&language=en&from={from}&sortBy=publishedAt&pageSize=20&apiKey={_options.ApiKey}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("NewsAPI вернул ошибку: {StatusCode}", response.StatusCode);
                return Enumerable.Empty<NewsArticleDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Articles?.Select(a => new NewsArticleDto
            {
                Title = a.Title ?? string.Empty,
                Description = a.Description ?? string.Empty,
                Source = new SourceInfo { Name = a.Source?.Name ?? "Unknown" },
                PublishedAt = DateTime.TryParse(a.PublishedAt, out var dt) ? dt : DateTime.UtcNow
            }) ?? Enumerable.Empty<NewsArticleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе к NewsAPI");
            return Enumerable.Empty<NewsArticleDto>();

        }
    }

    // Внутренние классы для десериализации ответа NewsAPI
    private class NewsApiResponse
    {
        public List<NewsApiArticle> Articles { get; set; } = new();
    }

    private class NewsApiArticle
    {
        public NewsApiSource Source { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PublishedAt { get; set; } = string.Empty;
    }

    private class NewsApiSource
    {
        public string Name { get; set; } = string.Empty;
    }
}