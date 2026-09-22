using System.Net;

namespace backend.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public override ErrorCode Code => ErrorCode.InvalidCredentials;
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}
