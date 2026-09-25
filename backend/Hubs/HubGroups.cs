namespace backend.Hubs
{
    public static class HubGroups
    {
        public static string User(Guid userId) => $"user:{userId}";

        public static string Work(Guid workId) => $"work:{workId}";
    }
}
