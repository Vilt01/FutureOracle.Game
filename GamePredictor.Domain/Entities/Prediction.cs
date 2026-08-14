using System;
using System.Collections.Generic;

namespace GamePredictor.Domain.Entities;

public partial class Prediction
{
    public int Id { get; set; }

    public decimal PredictedMetacritic { get; set; }

    public string SalesClas { get; set; } = null!;

    public decimal Confidence { get; set; }

    public string RiskLevel { get; set; } = null!;

    public string Arguments { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool? Verified { get; set; }

    public int GameId { get; set; }

    public virtual Game Game { get; set; } = null!;
}
