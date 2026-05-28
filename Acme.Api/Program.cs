using Acme.Api.Configuration;
using Acme.Infrastructure.Database.Sql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddMappings();
builder.Services.AddApiSwagger(builder.Environment, builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();
    db.Database.EnsureCreated();
}

app.UseApiPipeline();

app.Run();

public partial class Program;
