using System.Net;

namespace backend.Exceptions
{
    public class ForbiddenException : AppException
    {
        public override ErrorCode Code { get; }
        public override HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

        public ForbiddenException(ErrorCode code)
        {
            Code = code;
        }
    }
}
