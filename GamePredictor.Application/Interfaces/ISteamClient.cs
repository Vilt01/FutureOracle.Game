using System.Threading.Tasks;

namespace GamePredictor.Application.Interfaces;

public interface ISteamClient
{
    Task<int> GetWishlistCountAsync(int appId);
    Task<int?> FindAppIdByNameAsync(string gameName);
}
