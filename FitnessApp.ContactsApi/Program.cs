using System.Reflection;
using AutoMapper;
using FitnessApp.AzureServiceBus;
using FitnessApp.Common.Abstractions.Db.Configuration;
using FitnessApp.Common.Abstractions.Db.DbContext;
using FitnessApp.Common.Configuration.AppConfiguration;
using FitnessApp.Common.Configuration.Identity;
using FitnessApp.Common.Configuration.Mongo;
using FitnessApp.Common.Configuration.Swagger;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.ContactsApi;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.DependencyInjection;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.ServiceBus.AzureServiceBus.Configuration;
using FitnessApp.ServiceBus.AzureServiceBus.Consumer;
using FitnessApp.ServiceBus.AzureServiceBus.Producer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddTransient<IJsonSerializer, JsonSerializer>();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoConnection"));

BsonClassMap.RegisterClassMap<UserContactsCollectionEntity>(cm =>
{
    cm.MapMember(c => c.UserId);
    cm.MapMember(c => c.Collection);
});
BsonClassMap.RegisterClassMap<ContactCollectionItemEntity>(cm =>
{
    cm.MapMember(c => c.Id);
});

builder.Services.ConfigureMongoClient(builder.Configuration);

builder.Services.AddTransient<IDbContext<UserContactsCollectionEntity>, DbContext<UserContactsCollectionEntity>>();

builder.Services.AddContactsRepository();

builder.Services.Configure<AzureServiceBusSettings>(builder.Configuration.GetSection("AzureServiceBusSettings"));

builder.Services.AddTransient<IMessageProducer, MessageProducer>();

builder.Services.AddTransient<IContactsService, ContactsService>();

builder.Services.AddContactsMessageTopicSubscribersService();

builder.Services.AddSingleton<IMessageConsumer, MessageConsumer>();

builder.Services.AddHostedService<MessageListenerService>();

builder.Services.ConfigureAzureAdAuthentication(builder.Configuration);

builder.Services.ConfigureSwaggerConfiguration(Assembly.GetExecutingAssembly().GetName().Name);

builder.Host.ConfigureAzureAppConfiguration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger XML Api Demo v1");
});

app.Run();

#pragma warning disable S1118 // Utility classes should not have public constructor
public partial class Program { }
#pragma warning restore S1118 // Utility classes should not have public constructor
