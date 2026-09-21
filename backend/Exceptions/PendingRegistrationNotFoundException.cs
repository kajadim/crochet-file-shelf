namespace backend.Exceptions
{
    public class PendingRegistrationNotFoundException : Exception
    {
        public PendingRegistrationNotFoundException() : base("No pending registration found for this email.")
        { }
    }
}
