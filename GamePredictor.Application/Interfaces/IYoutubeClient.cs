using System.Threading.Tasks;

namespace GamePredictor.Application.Interfaces;

public interface IYoutubeClient
{
    Task<long> GetTrailerViewsAsync(string videoId);

    // 👇 НОВЫЙ МЕТОД
    Task<string?> FindTrailerIdAsync(string gameName);
}