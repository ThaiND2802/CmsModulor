using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(Guid Id) : IRequest;
