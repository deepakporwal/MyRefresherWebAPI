using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyRefresherWebAPI.Configuration;
using MyRefresherWebAPI.Models.Auth;
using MyRefresherWebAPI.Services;

namespace MyRefresherWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IOptions<DevelopmentCredentialsOptions> credentialsOptions,
    JwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> CreateToken(LoginRequest request)
    {
        var configuredCredentials = credentialsOptions.Value;
        var usernameMatches = FixedTimeEquals(request.Username, configuredCredentials.Username);
        var passwordMatches = FixedTimeEquals(request.Password, configuredCredentials.Password);

        if (!usernameMatches || !passwordMatches)
        {
            return Unauthorized();
        }

        return Ok(tokenService.CreateToken(configuredCredentials.Username));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(left),
            Encoding.UTF8.GetBytes(right));
    }
}
