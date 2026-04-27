using MediatR;

namespace Commerce.Modules.Catalog.Application.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest;
