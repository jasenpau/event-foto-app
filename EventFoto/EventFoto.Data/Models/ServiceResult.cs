using System.Net;
using EventFoto.Data.Enums;

namespace EventFoto.Data.Models;

public record ServiceResult<T>
{
    public bool Success { get; init; }
    public T Data { get; init; }
    public string ErrorMessage { get; init; }
    public HttpStatusCode? StatusCode { get; init; }

    public static ServiceResult<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
    };
    
    public static ServiceResult<T> Ok(T data, HttpStatusCode statusCode) => new()
    {
        Success = true,
        Data = data,
        StatusCode = statusCode
    };
    
    public static ServiceResult<T> Fail(string error) => new()
    {
        Success = false,
        ErrorMessage = error,
    };
    
    public static ServiceResult<T> Fail(string error, HttpStatusCode statusCode) => new()
    {
        Success = false,
        ErrorMessage = error,
        StatusCode = statusCode,
    };
}
