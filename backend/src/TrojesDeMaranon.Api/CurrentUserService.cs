using System.Security.Claims;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Api;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId => ReadGuid(ClaimTypes.NameIdentifier) ?? ReadGuid("sub");
    public Guid? CompanyId => ReadGuid("company_id");
    public string? Email => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    private Guid? ReadGuid(string claimType)
    {
        var value = accessor.HttpContext?.User.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
