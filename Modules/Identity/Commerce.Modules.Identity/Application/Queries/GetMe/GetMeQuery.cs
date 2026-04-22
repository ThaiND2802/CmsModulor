using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetMe;

public sealed record GetMeQuery : IRequest<ApiResponse<UserDto>>;
