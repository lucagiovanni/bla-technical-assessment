using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskManager.Api;

public static class CurrentUser
{
    public static Guid? Id(ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
