using System.Net;

namespace backend.Exceptions
{
    public class InvalidRefreshTokenException : AppException
    {
        public override ErrorCode Code => ErrorCode.InvalidRefreshToken;
        public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    }
}
