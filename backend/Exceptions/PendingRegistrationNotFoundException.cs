using System.Net;

namespace backend.Exceptions
{
    public class PendingRegistrationNotFoundException : AppException
    {
        public override ErrorCode Code => ErrorCode.PendingRegistrationNotFound;
        public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    }
}
