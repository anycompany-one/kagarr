using System.Threading.Tasks;
using Kagarr.Core.Deals;
using Kagarr.Core.Wishlist;

namespace Kagarr.Core.Notifications
{
    public interface INotificationService
    {
        Task OnGameAddedAsync(Games.Game game);
        Task OnGameFileImportedAsync(Games.Game game, string filePath);
        Task OnGameGrabbedAsync(Games.Game game, string releaseTitle);
        Task OnDealFoundAsync(WishlistItem item, GameDeal deal);
    }
}
