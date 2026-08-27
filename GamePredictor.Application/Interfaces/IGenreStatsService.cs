namespace GamePredictor.Application.Interfaces;

public interface IGenreStatsService
{
    Task<double> GetAverageScoreForGenreAsync(string genre);
}