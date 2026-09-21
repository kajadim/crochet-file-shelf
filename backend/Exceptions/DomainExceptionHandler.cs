using Microsoft.AspNetCore.Diagnostics;

namespace backend.Exceptions
{
    public class DomainExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var statusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => (int?)null,
            };

            if (statusCode is null)
            {
                return false;
            }

            httpContext.Response.StatusCode = statusCode.Value;
            await httpContext.Response.WriteAsJsonAsync(new { message = exception.Message }, cancellationToken);
            return true;
        }
    }
}
