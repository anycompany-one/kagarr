namespace Kagarr.Core.Notifications
{
    public interface INotificationService
    {
        void OnGameAdded(Games.Game game);
        void OnGameFileImported(Games.Game game, string filePath);
        void OnGameGrabbed(Games.Game game, string releaseTitle);
    }
}
