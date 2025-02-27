using System.ComponentModel.DataAnnotations;

namespace EventFoto.Data.Attributes;

public class AppRequired : ValidationAttribute
{
    public new static string ErrorMessage => "required";
    
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult(ErrorMessage);
        }
        
        var success = value is not string stringValue || !string.IsNullOrWhiteSpace(stringValue);
        return success ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }
}
