using System.Net;

namespace backend.Exceptions
{
    public class InvalidOrExpiredCodeException : AppException
    {
        public override ErrorCode Code => ErrorCode.InvalidOrExpiredCode;
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}
