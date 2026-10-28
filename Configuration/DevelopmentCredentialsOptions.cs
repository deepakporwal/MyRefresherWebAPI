using System.ComponentModel.DataAnnotations;

namespace MyRefresherWebAPI.Configuration;

public sealed class DevelopmentCredentialsOptions
{
    public const string SectionName = "DevelopmentCredentials";

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string Password { get; set; } = string.Empty;
}
