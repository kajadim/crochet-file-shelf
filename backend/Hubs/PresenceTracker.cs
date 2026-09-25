using System.Collections.Concurrent;

namespace backend.Hubs
{
    public sealed record PresenceUser(Guid UserId, string DisplayName);

    public class PresenceTracker
    {
        private readonly object _lock = new();
        private readonly Dictionary<Guid, Dictionary<string, PresenceUser>> _byWork = new();

        public void Join(Guid workId, string connectionId, PresenceUser user)
        {
            lock (_lock)
            {
                if (!_byWork.TryGetValue(workId, out var connections))
                {
                    connections = new Dictionary<string, PresenceUser>();
                    _byWork[workId] = connections;
                }
                connections[connectionId] = user;
            }
        }

        public bool Leave(Guid workId, string connectionId)
        {
            lock (_lock)
            {
                if (!_byWork.TryGetValue(workId, out var connections) || !connections.Remove(connectionId))
                {
                    return false;
                }
                if (connections.Count == 0)
                {
                    _byWork.Remove(workId);
                }
                return true;
            }
        }

        public List<Guid> RemoveConnection(string connectionId)
        {
            var affected = new List<Guid>();
            lock (_lock)
            {
                foreach (var (workId, connections) in _byWork.ToList())
                {
                    if (connections.Remove(connectionId))
                    {
                        affected.Add(workId);
                        if (connections.Count == 0)
                        {
                            _byWork.Remove(workId);
                        }
                    }
                }
            }
            return affected;
        }

        public List<string> RemoveUser(Guid workId, Guid userId)
        {
            lock (_lock)
            {
                if (!_byWork.TryGetValue(workId, out var connections))
                {
                    return [];
                }

                var removed = connections.Where(c => c.Value.UserId == userId).Select(c => c.Key).ToList();
                foreach (var connectionId in removed)
                {
                    connections.Remove(connectionId);
                }
                if (connections.Count == 0)
                {
                    _byWork.Remove(workId);
                }
                return removed;
            }
        }

        public List<PresenceUser> Users(Guid workId)
        {
            lock (_lock)
            {
                return _byWork.TryGetValue(workId, out var connections)
                    ? connections.Values.DistinctBy(u => u.UserId).OrderBy(u => u.DisplayName).ToList()
                    : [];
            }
        }
    }
}
