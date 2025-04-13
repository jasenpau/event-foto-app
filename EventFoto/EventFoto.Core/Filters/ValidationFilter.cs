using EventFoto.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventFoto.Core.Filters;

public class ValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var ers = context.ModelState
            .SelectMany(modelStateEntry =>
                modelStateEntry.Value?.Errors.Select(e =>
                    new ValidationError
                    {
                        Field = ToJsonProperty(modelStateEntry.Key),
                        Error = e.ErrorMessage
                    }) ?? [])
            .ToList();

        context.Result = new BadRequestObjectResult(new
        {
            Message = AppErrorMessage.ValidationFailed,
            Errors = ers
        });
    }

    private record ValidationError
    {
        public required string Field { get; init; }
        public required string Error { get; init; }
    }

    private string ToJsonProperty(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }
}
