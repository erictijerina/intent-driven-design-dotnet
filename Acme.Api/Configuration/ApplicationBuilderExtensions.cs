using Acme.Domain.Abstractions;
using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;

namespace Acme.Api.Configuration;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiPipeline(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (!env.IsDevelopment())
            app.UseHsts();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            IntentDispatcher.Configure(context.RequestServices.GetRequiredService<IIntentRuntime>());
            EventPublisher.Configure(context.RequestServices.GetRequiredService<IEventRuntime>());
            try
            {
                await next();
            }
            finally
            {
                IntentDispatcher.Configure(null);
                EventPublisher.Configure(null);
            }
        });

        app.UseApiSwagger();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        return app;
    }
}
