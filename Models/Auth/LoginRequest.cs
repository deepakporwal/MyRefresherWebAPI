using System.ComponentModel.DataAnnotations;

namespace MyRefresherWebAPI.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
