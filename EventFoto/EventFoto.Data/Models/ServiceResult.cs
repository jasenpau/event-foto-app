using EventFoto.Data.Enums;

namespace EventFoto.Data.Models;

public record ServiceResult<T>
{
    public bool Success { get; init; }
    public AppError Error { get; init; }
    public T Data { get; init; }

    public string ErrorMessage => AppErrorMessage.Get(Error);


    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };
    
    public static ServiceResult<T> Fail(AppError error) => new()
    {
        Success = false,
        Error = error,
    };
}
