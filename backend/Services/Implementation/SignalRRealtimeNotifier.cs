using backend.Dtos.Notifications;
using backend.Hubs;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace backend.Services.Implementation
{
    public class SignalRRealtimeNotifier : IRealtimeNotifier
    {
        public const string ConnectionIdHeader = "X-Connection-Id";

        private readonly IHubContext<AppHub> _hub;
        private readonly PresenceTracker _presence;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SignalRRealtimeNotifier(
            IHubContext<AppHub> hub,
            PresenceTracker presence,
            IHttpContextAccessor httpContextAccessor)
        {
            _hub = hub;
            _presence = presence;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task CellsChangedAsync(Guid workId, IReadOnlyList<CellEvent> cells) =>
            ToWorkAsync(workId, "CellsChanged", new { workId, cells });

        public Task PositionChangedAsync(Guid workId, int row, int column) =>
            ToWorkAsync(workId, "PositionChanged", new { workId, row, column });

        public Task ActiveRowChangedAsync(Guid workId, int? row) =>
            ToWorkAsync(workId, "ActiveRowChanged", new { workId, row });

        public Task PatternResetAsync(Guid workId) =>
            ToWorkAsync(workId, "PatternReset", new { workId });

        public Task CommentsChangedAsync(Guid workId) =>
            ToWorkAsync(workId, "CommentsChanged", new { workId });

        public async Task AccessChangedAsync(Guid userId, Guid workId, string? role)
        {
            await _hub.Clients.Group(HubGroups.User(userId)).SendAsync("AccessChanged", new { workId, role });

            if (role is not null)
            {
                return;
            }

            var connectionIds = _presence.RemoveUser(workId, userId);
            foreach (var connectionId in connectionIds)
            {
                await _hub.Groups.RemoveFromGroupAsync(connectionId, HubGroups.Work(workId));
            }

            if (connectionIds.Count > 0)
            {
                await _hub.Clients.Group(HubGroups.Work(workId)).SendAsync(
                    "PresenceChanged",
                    new { workId, users = _presence.Users(workId) });
            }
        }

        public Task NotificationReceivedAsync(Guid userId, NotificationResponse notification) =>
            _hub.Clients.Group(HubGroups.User(userId)).SendAsync("NotificationReceived", notification);

        private Task ToWorkAsync(Guid workId, string method, object payload)
        {
            var senderConnectionId = _httpContextAccessor.HttpContext?.Request.Headers[ConnectionIdHeader].ToString();
            var group = HubGroups.Work(workId);

            var clients = string.IsNullOrEmpty(senderConnectionId)
                ? _hub.Clients.Group(group)
                : _hub.Clients.GroupExcept(group, senderConnectionId);

            return clients.SendAsync(method, payload);
        }
    }
}
