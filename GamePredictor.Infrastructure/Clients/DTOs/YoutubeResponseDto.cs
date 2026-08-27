using System.Text.Json.Serialization;

namespace GamePredictor.Infrastructure.Clients.DTOs;

public class YoutubeResponseDto
{
    [JsonPropertyName("items")]
    public List<YoutubeItemDto> Items { get; set; } = new();
}

public class YoutubeItemDto
{
    [JsonPropertyName("statistics")]
    public YoutubeStatisticsDto Statistics { get; set; } = new();
}

public class YoutubeStatisticsDto
{
    [JsonPropertyName("viewCount")]
    public string ViewCount { get; set; } = "0";
}