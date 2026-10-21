using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyLeitner.Domain.Users;

namespace MyLeitner.Identity;

public interface IJwtTokenService
{
    string Create(User user);
}

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public const string SuperUserClaim = "is_super";

    public string Create(User user)
    {
        var o = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            Expires = DateTime.UtcNow.AddMinutes(o.AccessTokenMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                ["sub"] = user.Id.ToString(),
                ["email"] = user.Email,
                ["fullname"] = user.DisplayName,
                [SuperUserClaim] = user.IsSuperUser.ToString().ToLowerInvariant()
            }
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
