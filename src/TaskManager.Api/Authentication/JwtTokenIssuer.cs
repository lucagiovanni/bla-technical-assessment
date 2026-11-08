using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TaskManager.Api.Authentication;

public sealed class JwtTokenIssuer
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtTokenIssuer(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _credentials = new SigningCredentials(SigningKey.Create(_options.SigningKey), SecurityAlgorithms.HmacSha256);
    }

    public string Issue(Guid userId, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: _credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
