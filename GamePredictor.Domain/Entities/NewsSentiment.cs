using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class NewsSentiment
{
    public int Id { get; set; }

    public string Source { get; set; } = null!;

    public DateTime PublishedAt { get; set; }

    public decimal? SentimentScore { get; set; }

    public decimal? Relevance { get; set; }

    public string? Keywords { get; set; }

    public int GameId { get; set; }

    public virtual Game Game { get; set; } = null!;
}
