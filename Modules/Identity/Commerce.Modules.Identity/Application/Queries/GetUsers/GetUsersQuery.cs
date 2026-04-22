using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetUsers;

public sealed class GetUsersQuery : PagedListRequest, IRequest<PagedApiResponse<UserDto>>;
