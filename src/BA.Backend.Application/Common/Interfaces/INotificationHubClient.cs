namespace BA.Backend.Application.Common.Interfaces;

public interface INotificationHubClient
{
    Task ReceiveNotification(string titulo, string mensaje, string tipo);
    Task ReceiveMessage(string user, string message);
}
