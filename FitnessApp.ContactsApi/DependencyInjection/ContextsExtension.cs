using System;
using System.Linq;
using FitnessApp.Common.Abstractions.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SharpCompress;

namespace FitnessApp.ContactsApi.DependencyInjection;

public static class ContextsExtension
{
    public static IServiceCollection ConfigureContexts(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnectionSection = configuration.GetSection("MongoConnection");
        var mongoDatabse = mongoConnectionSection.GetSection("DatabaseName").Value!;
        var connectionString = mongoConnectionSection.GetSection("ConnectionString").Value!;
        var contexts = configuration
            .GetSection("Contexts")
            .GetChildren()
            .Select(value => value.GetValue<string>("CollecttionName"));
        foreach (var context in contexts)
        {
            services.Configure<MongoDbSettings>(context, options =>
            {
                options.ConnectionString = connectionString;
                options.DatabaseName = mongoDatabse;
                options.CollecttionName = context;
            });
        }

        services.AddTransient<IMongoClient, MongoClient>((IServiceProvider sp) => new MongoClient(connectionString));
        return services;
    }
}
