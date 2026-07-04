namespace TrojesDeMaranon.Application.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, MeResponse User);
public sealed record MeResponse(Guid Id, Guid CompanyId, string FullName, string Email, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
