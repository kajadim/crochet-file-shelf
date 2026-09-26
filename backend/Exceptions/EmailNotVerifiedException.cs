using System.Net;

namespace backend.Exceptions
{
    public class EmailNotVerifiedException : AppException
    {
        public override ErrorCode Code => ErrorCode.EmailNotVerified;
        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;
    }
}
