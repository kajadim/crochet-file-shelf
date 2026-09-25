using backend.Exceptions;
using backend.Extensions;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        private readonly IWorkAccessService _access;
        private readonly IUserRepository _userRepository;
        private readonly PresenceTracker _presence;

        public AppHub(IWorkAccessService access, IUserRepository userRepository, PresenceTracker presence)
        {
            _access = access;
            _userRepository = userRepository;
            _presence = presence;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.User(UserId));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var workId in _presence.RemoveConnection(Context.ConnectionId))
            {
                await BroadcastPresenceAsync(workId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinWork(Guid workId)
        {
            try
            {
                await _access.RequireAsync(UserId, workId, WorkAccessLevel.Read);
            }
            catch (AppException)
            {
                throw new HubException("access-denied");
            }

            var user = await _userRepository.GetByIdAsync(UserId) ?? throw new HubException("access-denied");

            await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Work(workId));
            _presence.Join(workId, Context.ConnectionId, new PresenceUser(user.Id, user.DisplayName));
            await BroadcastPresenceAsync(workId);
        }

        public async Task LeaveWork(Guid workId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroups.Work(workId));
            if (_presence.Leave(workId, Context.ConnectionId))
            {
                await BroadcastPresenceAsync(workId);
            }
        }

        private Guid UserId => Context.User!.GetUserId();

        private Task BroadcastPresenceAsync(Guid workId) =>
            Clients.Group(HubGroups.Work(workId)).SendAsync(
                "PresenceChanged",
                new { workId, users = _presence.Users(workId) });
    }
}
