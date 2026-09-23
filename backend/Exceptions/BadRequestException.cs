using System.Net;

namespace backend.Exceptions
{
    public class BadRequestException : AppException
    {
        public override ErrorCode Code { get; }
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        public BadRequestException(ErrorCode code)
        {
            Code = code;
        }
    }
}
