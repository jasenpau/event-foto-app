namespace EventFoto.Data.Enums;

public static class AppErrorMessage
{
    public const string InternalError = "internal-error";
    public const string UserNotFound = "user-not-found";
    public const string NoCredentials = "no-credentials";
    public const string InvalidCredentials = "invalid-credentials";

    public static Dictionary<AppError, string> Map = new()
    {
        { AppError.UserNotFound, UserNotFound },
        { AppError.NoCredentials, NoCredentials },
        { AppError.InvalidCredentials, InvalidCredentials }
    };

    public static string Get(AppError code) => Map[code];
}
