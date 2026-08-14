using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class PreRelelaseMetric
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public int WishlistCount { get; set; }

    public int? TwitchViewerAvg { get; set; }

    public long? YoutubeTrailerViews { get; set; }

    public int RedditMentions { get; set; }

    public int GameId { get; set; }

    public virtual Game Game { get; set; } = null!;
}
