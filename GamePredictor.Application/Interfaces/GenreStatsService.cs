using GamePredictor.Application.Interfaces;
using GamePredictor.Domain.Interfaces;

namespace GamePredictor.Application.Services;

public class GenreStatsService : IGenreStatsService
{
    private readonly IGameRepository _gameRepository;

    public GenreStatsService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<double> GetAverageScoreForGenreAsync(string genre)
    {
        return await _gameRepository.GetGenreAverageAsync(genre);
    }
}