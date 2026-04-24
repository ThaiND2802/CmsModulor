using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Infrastructure.Authentication;

public interface IJwtTokenService
{
    string CreateToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions, DateTime expiresAtUtc);

    DateTime GetExpirationUtc();
}
