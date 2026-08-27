using System.Xml.Linq;
using GamePredictor.Application.DTOs;
using GamePredictor.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GamePredictor.Infrastructure.Clients;

public class RssNewsClient : INewsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RssNewsClient> _logger;

    public RssNewsClient(HttpClient httpClient, ILogger<RssNewsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsArticleDto>> GetNewsForGameAsync(string gameTitle, int daysBack = 60)
    {
        try
        {
            var rssUrl = $"https://news.google.com/rss/search?q={Uri.EscapeDataString(gameTitle + " game")}&hl=en-US&gl=US&ceid=US:en";
            var xml = await _httpClient.GetStringAsync(rssUrl);
            var doc = XDocument.Parse(xml);
            var items = doc.Descendants("item")
                .Select(item => new NewsArticleDto
                {
                    Title = item.Element("title")?.Value ?? "",
                    Description = item.Element("description")?.Value ?? "",
                    Source = new SourceInfo { Name = "Google News" },
                    PublishedAt = DateTime.TryParse(item.Element("pubDate")?.Value, out var dt) ? dt : DateTime.UtcNow
                })
                .Take(10);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка RSS для {GameTitle}", gameTitle);
            return Enumerable.Empty<NewsArticleDto>();
        }
    }
}