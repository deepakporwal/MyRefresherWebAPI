using System.ComponentModel.DataAnnotations;

namespace MyRefresherWebAPI.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class GreaterThanZeroAttribute : ValidationAttribute
{
    public GreaterThanZeroAttribute()
        : base("{0} must be greater than zero.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is int intValue && intValue > 0
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}
