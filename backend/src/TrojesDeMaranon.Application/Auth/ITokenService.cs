using TrojesDeMaranon.Domain.Security;

namespace TrojesDeMaranon.Application.Auth;

public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
