namespace Commerce.Modules.Identity.Infrastructure.Authentication;

public interface IPasswordHasherService
{
    string Hash(string password);

    bool Verify(string hash, string password);
}
