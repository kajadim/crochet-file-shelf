using System.Net;

namespace backend.Exceptions
{
    public class EmailAlreadyExistsException : AppException
    {
        public override ErrorCode Code => ErrorCode.EmailAlreadyExists;
        public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    }
}
