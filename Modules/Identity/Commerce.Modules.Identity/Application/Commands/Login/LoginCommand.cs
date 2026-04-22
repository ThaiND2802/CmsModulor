using Commerce.Modules.Identity.Application.DTOs.Requests;

namespace Commerce.Modules.Identity.Application.Commands.Login;

public sealed record LoginCommand(LoginRequest Request);
