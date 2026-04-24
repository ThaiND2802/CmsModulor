using System.Security.Cryptography;
using System.Text;
using CommerceCore.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Commerce.Modules.Identity.Infrastructure.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

        _options = options.Value;

        if (_options.RefreshTokenDays <= 0)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenDays must be greater than zero.");
        }
    }

    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(bytes);
    }

    public DateTime GetExpirationUtc()
    {
        return _dateTimeProvider.UtcNow.AddDays(_options.RefreshTokenDays);
    }
}
