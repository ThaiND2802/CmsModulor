using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Commerce.Modules.Identity.Domain;
using CommerceCore.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Commerce.Modules.Identity.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private const string PreferredUserNameClaimType = "preferred_username";

    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

        _options = options.Value;
        ValidateOptions(_options);
    }

    public string CreateToken(User user, DateTime expiresAtUtc)
    {
        ArgumentNullException.ThrowIfNull(user);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userId = user.Id.ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(PreferredUserNameClaimType, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _dateTimeProvider.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpirationUtc()
    {
        return _dateTimeProvider.UtcNow.AddMinutes(_options.AccessTokenMinutes);
    }

    private static void ValidateOptions(JwtOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Audience);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SigningKey);

        if (options.AccessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be greater than zero.");
        }
    }
}
