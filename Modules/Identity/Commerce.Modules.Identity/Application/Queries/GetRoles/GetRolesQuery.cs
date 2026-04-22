using Commerce.Modules.Identity.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Identity.Application.Queries.GetRoles;

public sealed class GetRolesQuery : PagedListRequest, IRequest<PagedApiResponse<RoleDto>>;
