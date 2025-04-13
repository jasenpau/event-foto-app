using System.Net;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.Core.Extensions;

public static class ServiceResultExtensions
{
    public static ObjectResult ToErrorResponse<T>(this ServiceResult<T> result)
    {
        var problemDetails = ToProblemDetails(result);
        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => new NotFoundObjectResult(problemDetails),
            HttpStatusCode.Conflict => new ConflictObjectResult(problemDetails),
            _ => new BadRequestObjectResult(problemDetails)
        };
    }

    private static ProblemDetails ToProblemDetails<T>(ServiceResult<T> result)
    {
        return new ProblemDetails
        {
            Status = (int?)result.StatusCode,
            Title = result.ErrorMessage,
        };
    }
}
