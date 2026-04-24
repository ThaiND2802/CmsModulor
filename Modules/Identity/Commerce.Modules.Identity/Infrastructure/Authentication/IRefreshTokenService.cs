namespace Commerce.Modules.Identity.Infrastructure.Authentication;

public interface IRefreshTokenService
{
    string GenerateToken();

    string HashToken(string token);

    DateTime GetExpirationUtc();
}
