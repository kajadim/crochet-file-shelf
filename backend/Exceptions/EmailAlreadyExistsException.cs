namespace backend.Exceptions
{
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException() : base("An account with this email already exists.")
        { }
    }
}
