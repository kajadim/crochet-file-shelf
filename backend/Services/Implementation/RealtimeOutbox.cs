using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class RealtimeOutbox : IRealtimeOutbox
    {
        private readonly IRealtimeNotifier _notifier;
        private readonly ILogger<RealtimeOutbox> _logger;
        private readonly List<Func<IRealtimeNotifier, Task>> _pending = [];

        public RealtimeOutbox(IRealtimeNotifier notifier, ILogger<RealtimeOutbox> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public void Enqueue(Func<IRealtimeNotifier, Task> action) => _pending.Add(action);

        public async Task FlushAsync()
        {
            var actions = _pending.ToList();
            _pending.Clear();

            foreach (var action in actions)
            {
                try
                {
                    await action(_notifier);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deliver a real-time event.");
                }
            }
        }
    }
}
