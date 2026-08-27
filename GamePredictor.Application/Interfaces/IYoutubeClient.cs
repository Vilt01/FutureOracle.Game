using System.Threading.Tasks;

namespace GamePredictor.Application.Interfaces;

public interface IYoutubeClient
{
    Task<long> GetTrailerViewsAsync(string videoId);
    Task<string?> FindTrailerIdAsync(string gameName);
}
