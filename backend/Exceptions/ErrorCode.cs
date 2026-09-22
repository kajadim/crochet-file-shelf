namespace backend.Exceptions
{
    public enum ErrorCode
    {
        ParentFolderNotFound,
        FolderNotFound,
        FolderNameConflict,
        WorkNotFound,
        ColorNotFound,
        ColorNameConflict,
        EmailAlreadyExists,
        InvalidCredentials,
        InvalidRefreshToken,
        InvalidOrExpiredCode,
        PendingRegistrationNotFound,
    }
}
