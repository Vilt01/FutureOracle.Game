namespace GamePredictor.Application.DTOs;

public class NewsArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SourceInfo Source { get; set; } = new();
    public DateTime PublishedAt { get; set; }
}

public class SourceInfo
{
    public string Name { get; set; } = string.Empty;
}