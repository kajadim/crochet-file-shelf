namespace backend.Options
{
    public class RefreshTokenOptions
    {
        public const string SectionName = "RefreshToken";

        public int ExpiryDays { get; set; }
    }
}
