using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Filters
{
    public class RealtimeFlushFilter : IAsyncActionFilter
    {
        private readonly IRealtimeOutbox _outbox;

        public RealtimeFlushFilter(IRealtimeOutbox outbox)
        {
            _outbox = outbox;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            if (executed.Exception is null)
            {
                await _outbox.FlushAsync();
            }
        }
    }
}
