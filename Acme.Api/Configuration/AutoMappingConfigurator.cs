using Acme.Application.Mappings;

namespace Acme.Api.Configuration;

public static class AutoMappingConfigurator
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Profile).Assembly);
        return services;
    }
}
