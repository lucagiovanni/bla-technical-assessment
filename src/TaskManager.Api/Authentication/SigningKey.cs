using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskManager.Api.Authentication;

public static class SigningKey
{
    public static SymmetricSecurityKey Create(string signingKey)
    {
        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }
}
