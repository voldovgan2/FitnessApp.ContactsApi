using System.Reflection;
using FitnessApp.Common.Configuration;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Services;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureMongo(builder.Configuration);
builder.Services.AddStackExchangeRedisCache(option => option.Configuration = builder.Configuration["Redis:Configuration"]);
builder.Services.AddTransient<IGlobalContainer, GlobalContainer>();
builder.Services.AddTransient<IFollowersContainer, FollowersContainer>();
builder.Services.AddTransient<IContactsRepository, ContactsRepository>();
builder.Services.AddTransient<IStorage, Storage>();
builder.Services.AddTransient<IContactsService, ContactsService>();
builder.Services.AddTransient<IUsersCache, UsersCache>();
builder.Services.AddTransient<IUserDbContext, UserDbContext>();
builder.Services.AddTransient<IFollowerDbContext, FollowerDbContext>();
builder.Services.AddTransient<IFollowingDbContext, FollowingDbContext>();
builder.Services.AddTransient<IFollowerRequestDbContext, FollowerRequestDbContext>();
builder.Services.AddTransient<IFirstCharSearchUserDbContext, FirstCharSearchUserDbContext>();
builder.Services.AddTransient<IFirstCharDbContext, FirstCharDbContext>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();

builder.Services.ConfigureNats(builder.Configuration);

// builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureSwagger(Assembly.GetExecutingAssembly().GetName().Name);

builder.Host.ConfigureAppConfiguration();
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
