using GamePredictor.Application.DTOs;

namespace GamePredictor.Application.Interfaces;

public interface INewsApiClient
{
    Task<IEnumerable<NewsArticleDto>> GetNewsForGameAsync(string gameTitle, int daysBack = 60);
}