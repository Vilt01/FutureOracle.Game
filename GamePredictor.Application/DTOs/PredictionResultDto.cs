namespace GamePredictor.Application.DTOs;

public class PredictionResultDto
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public double PredictedScore { get; set; }
    public string SalesClass { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}