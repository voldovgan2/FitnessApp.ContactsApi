using System.Reflection;
using FitnessApp.Common.Configuration;
using FitnessApp.ContactsApi;
using FitnessApp.ContactsApi.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureMapper(new MappingProfile());
builder.Services.ConfigureMongo(builder.Configuration);
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = builder.Configuration["Redis:Configuration"];
});
builder.Services.ConfigureStorage();

// builder.Services.ConfigureNats(builder.Configuration);
// builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureSwagger(Assembly.GetExecutingAssembly().GetName().Name);

// builder.Host.ConfigureAppConfiguration();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerAndUi();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
public partial class Program { }
