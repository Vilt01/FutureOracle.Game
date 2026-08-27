using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class Game
{
    public int? RawgId { get; set; }
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Genre { get; set; } = null!;

    public DateOnly? Releasedate { get; set; }

    public string? Platforms { get; set; }

    public decimal? BudgetEstimate { get; set; }

    public int? MetacriticScore { get; set; }

    public bool IsReleased { get; set; }

    public int? SteamAppId { get; set; }

    public string? TrailerYoutubeId { get; set; }

    public int DeveloperId { get; set; }

    public virtual Developers Developer { get; set; } = null!;

    public virtual ICollection<NewsSentiment> NewsSentiments { get; set; } = new List<NewsSentiment>();

    public virtual ICollection<PreReleaseMetrics> PreReleaseMetrics { get; set; } = new List<PreReleaseMetrics>();

    public virtual ICollection<Predictions> Predictions { get; set; } = new List<Predictions>();
}
