using System.Threading.Tasks;

namespace GamePredictor.Application.Interfaces;

public interface ISteamClient
{
    Task<int> GetWishlistCountAsync(int appId);

    // 👇 НОВЫЙ МЕТОД
    Task<int?> FindAppIdByNameAsync(string gameName);
}