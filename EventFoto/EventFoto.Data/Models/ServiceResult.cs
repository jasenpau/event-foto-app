using System.Net;
using EventFoto.Data.Enums;

namespace EventFoto.Data.Models;

public record ServiceResult<T>
{
    public bool Success { get; init; }
    public T Data { get; init; }
    public string ErrorMessage { get; init; }
    public string ErrorKey { get; init; }
    public HttpStatusCode? StatusCode { get; init; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };

    public static ServiceResult<T> Fail(string error, HttpStatusCode statusCode) => new()
    {
        Success = false,
        ErrorMessage = error,
        StatusCode = statusCode,
    };
    
    public static ServiceResult<T> Fail(string error, string errorKey, HttpStatusCode statusCode) => new()
    {
        Success = false,
        ErrorMessage = error,
        ErrorKey = errorKey,
        StatusCode = statusCode,
    };

    public static ServiceResult<T> Fail<TSource>(ServiceResult<TSource> result) => new()
    {
        Success = false,
        ErrorMessage = result.ErrorMessage,
        ErrorKey = result.ErrorKey,
        StatusCode = result.StatusCode,
    };
}
