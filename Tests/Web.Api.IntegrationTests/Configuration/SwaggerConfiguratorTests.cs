using Acme.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Web.Api.IntegrationTests.Configuration;

public sealed class SwaggerConfiguratorTests
{
    [Fact]
    public void AddApiSwagger_RegistersDocument_WhenNonProd()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Environment"] = "dev" })
            .Build();

        var hostEnvironment = new HostEnvironment { EnvironmentName = Environments.Development };

        services.AddApiSwagger(hostEnvironment, configuration);

        Assert.NotEmpty(services);
    }

    private sealed class HostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
