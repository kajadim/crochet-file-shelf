using System.Net;

namespace backend.Exceptions
{
    public abstract class AppException : Exception
    {
        public abstract ErrorCode Code { get; }
        public abstract HttpStatusCode StatusCode { get; }
    }
}
