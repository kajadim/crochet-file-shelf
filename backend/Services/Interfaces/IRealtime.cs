using backend.Dtos.Notifications;

namespace backend.Services.Interfaces
{
    public sealed record CellEvent(int Row, int Column, Guid? ColorId, string? HexValue);

    public interface IRealtimeNotifier
    {
        Task CellsChangedAsync(Guid workId, IReadOnlyList<CellEvent> cells);
        Task PositionChangedAsync(Guid workId, int row, int column);
        Task ActiveRowChangedAsync(Guid workId, int? row);
        Task PatternResetAsync(Guid workId);
        Task CommentsChangedAsync(Guid workId);
        Task AccessChangedAsync(Guid userId, Guid workId, string? role);
        Task NotificationReceivedAsync(Guid userId, NotificationResponse notification);
    }

    public interface IRealtimeOutbox
    {
        void Enqueue(Func<IRealtimeNotifier, Task> action);
        Task FlushAsync();
    }
}
