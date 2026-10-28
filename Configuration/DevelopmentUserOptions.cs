using System.ComponentModel.DataAnnotations;

namespace MyRefresherWebAPI.Configuration;

public sealed class DevelopmentUserOptions
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public string Password { get; set; } = string.Empty;

    [MinLength(1)]
    public List<string> Roles { get; set; } = [];
}
