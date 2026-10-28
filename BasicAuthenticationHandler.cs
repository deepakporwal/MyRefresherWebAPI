using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MyRefresherWebAPI;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ExpectedUsername = "admin";
    private const string ExpectedPassword = "password";

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader) ||
            !authorizationHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            var encodedCredentials = authorizationHeader.ToString()["Basic ".Length..].Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = credentials.IndexOf(':');

            if (separatorIndex < 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Basic authentication credentials."));
            }

            var username = credentials[..separatorIndex];
            var password = credentials[(separatorIndex + 1)..];

            if (!string.Equals(username, ExpectedUsername, StringComparison.Ordinal) ||
                !string.Equals(password, ExpectedPassword, StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password."));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic authentication credentials."));
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Basic realm=\"MyRefresherWebAPI\"";
        return Task.CompletedTask;
    }
}
