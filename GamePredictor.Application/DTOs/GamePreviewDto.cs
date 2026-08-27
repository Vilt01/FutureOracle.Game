namespace GamePredictor.Application.DTOs;

public class GamePreviewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public DateOnly? ReleaseDate { get; set; }
    public double? PredictedScore { get; set; }
    public double? Confidence { get; set; }
}