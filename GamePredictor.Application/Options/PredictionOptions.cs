namespace GamePredictor.Application.Options;


public class PredictionOptions
{
    public double StudioWeight { get; set; } = 0.6;
    public double GenreWeight { get; set; } = 0.4;
    public int DefaultStudioScore { get; set; } = 70;
    public int BlockbusterWishlistThreshold { get; set; } = 100000;
    public int AverageWishlistThreshold { get; set; } = 30000;
    public int HighViewsThreshold { get; set; } = 1000000;
    public int LowViewsThreshold { get; set; } = 100000;
}