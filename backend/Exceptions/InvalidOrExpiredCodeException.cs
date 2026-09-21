namespace backend.Exceptions
{
    public class InvalidOrExpiredCodeException : Exception
    {
        public InvalidOrExpiredCodeException() : base("Code is invalid or has expired.")
        { }
    }
}
