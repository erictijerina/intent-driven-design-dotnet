namespace Acme.Api.Configuration;

public static class SwaggerConfigurator
{
    public static IServiceCollection AddApiSwagger(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        if (IsSwaggerEnabled(environment, configuration))
            services.AddOpenApiDocument(document => document.Title = "Acme.Api");

        return services;
    }

    public static IApplicationBuilder UseApiSwagger(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        var config = app.ApplicationServices.GetRequiredService<IConfiguration>();

        if (IsSwaggerEnabled(env, config))
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        return app;
    }

    private static bool IsSwaggerEnabled(IHostEnvironment environment, IConfiguration configuration)
    {
        var configuredEnvironment = configuration.GetValue<string>("Environment");
        if (!string.IsNullOrWhiteSpace(configuredEnvironment))
            return !configuredEnvironment.Equals("prod", StringComparison.OrdinalIgnoreCase);

        return !environment.IsProduction();
    }
}
