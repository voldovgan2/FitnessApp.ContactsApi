using System.Reflection;
using FitnessApp.Common.Configuration;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.ContactsApi;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.DependencyInjection;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.ContactsApi.Services.MessageBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureMapper(new MappingProfile());

BsonClassMap.RegisterClassMap<UserContactsCollectionEntity>(cm =>
{
    cm.MapMember(c => c.UserId);
    cm.MapMember(c => c.Collection);
});
BsonClassMap.RegisterClassMap<ContactCollectionItemEntity>(cm =>
{
    cm.MapMember(c => c.Id);
});
builder.Services.AddTransient<IJsonSerializer, JsonSerializer>();
builder.Services.ConfigureMongo(builder.Configuration);
builder.Services.ConfigureVault(builder.Configuration);
builder.Services.ConfigureContactsRepository();
builder.Services.ConfigureNats(builder.Configuration);
builder.Services.AddContactsMessageTopicSubscribersService();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureSwagger(Assembly.GetExecutingAssembly().GetName().Name);
builder.Services.AddTransient<IContactsService, ContactsService>();
if ("false".Contains("true"))
    builder.Services.AddHostedService<ContactsMessageTopicSubscribersService>();

builder.Host.ConfigureAppConfiguration();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerAndUi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }
