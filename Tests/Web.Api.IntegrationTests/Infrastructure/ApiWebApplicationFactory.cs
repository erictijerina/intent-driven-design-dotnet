using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Web.Api.IntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _mongoConnectionString;

    public ApiWebApplicationFactory(string connectionString, string mongoConnectionString)
    {
        _connectionString = connectionString;
        _mongoConnectionString = mongoConnectionString;
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:AcmeContext"] = _connectionString,
                ["ConnectionStrings:AcmeMongo"] = _mongoConnectionString,
                ["Environment"] = "dev"
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
