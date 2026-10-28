namespace MyRefresherWebAPI.Models.Auth;

public sealed record TokenResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc);
