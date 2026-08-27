namespace GamePredictor.Application.Interfaces;

public interface ISentimentClient
{
    Task<double> GetSentimentAsync(string text);
}