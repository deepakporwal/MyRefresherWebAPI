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
    IOptions<DevelopmentUsersOptions> usersOptions,
    JwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> CreateToken(LoginRequest request)
    {
        var configuredUser = usersOptions.Value.Users.FirstOrDefault(user =>
            string.Equals(user.Username, request.Username, StringComparison.Ordinal));
        var passwordMatches = configuredUser is not null &&
            FixedTimeEquals(request.Password, configuredUser.Password);

        if (configuredUser is null || !passwordMatches)
        {
            return Unauthorized();
        }

        return Ok(tokenService.CreateToken(configuredUser));
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(left),
            Encoding.UTF8.GetBytes(right));
    }
}
