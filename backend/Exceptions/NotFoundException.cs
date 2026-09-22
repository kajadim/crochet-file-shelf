using System.Net;

namespace backend.Exceptions
{
    public class NotFoundException : AppException
    {
        public override ErrorCode Code { get; }
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

        public NotFoundException(ErrorCode code)
        {
            Code = code;
        }
    }
}
