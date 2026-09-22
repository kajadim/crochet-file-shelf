using System.Net;

namespace backend.Exceptions
{
    public class ConflictException : AppException
    {
        public override ErrorCode Code { get; }
        public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

        public ConflictException(ErrorCode code)
        {
            Code = code;
        }
    }
}
