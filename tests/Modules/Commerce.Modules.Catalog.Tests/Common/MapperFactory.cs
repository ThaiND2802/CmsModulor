using AutoMapper;
using Commerce.Modules.Catalog.Application.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Commerce.Modules.Catalog.Tests.Common;

internal static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<CatalogMappingProfile>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }
}
