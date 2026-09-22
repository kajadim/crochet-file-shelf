using backend.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;

namespace backend.Exceptions
{
    public class DomainExceptionHandler : IExceptionHandler
    {
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public DomainExceptionHandler(IStringLocalizer<ErrorMessages> localizer)
        {
            _localizer = localizer;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not AppException appException)
            {
                return false;
            }

            httpContext.Response.StatusCode = (int)appException.StatusCode;
            var message = _localizer[appException.Code.ToString()].Value;
            await httpContext.Response.WriteAsJsonAsync(new { message }, cancellationToken);
            return true;
        }
    }
}
