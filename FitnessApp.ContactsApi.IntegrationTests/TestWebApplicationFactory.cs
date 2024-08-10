using FitnessApp.Common.Abstractions.Db.Entities.Generic;
using FitnessApp.Common.IntegrationTests;
using FitnessApp.ContactsApi.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace FitnessApp.ContactsApi.IntegrationTests;
public class TestWebApplicationFactory(
    MongoDbFixture fixture,
    string databaseName,
    string collecttionName,
    string[] ids
    ) :
    TestGenericWebApplicationFactoryBase2<
        Program,
        MockAuthenticationHandler,
        UserContactsCollectionEntity>(
        fixture,
        databaseName,
        collecttionName,
        ids);

public class TestGenericWebApplicationFactoryBase2<
    TProgram,
    TAuthenticationHandler,
    TEntity
    >(
    MongoDbFixtureBase<TEntity> fixture,
    string databaseName,
    string collecttionName,
    string[] ids) :
    TestWebApplicationFactoryBase<TProgram, TAuthenticationHandler>
    where TProgram : class
    where TAuthenticationHandler : MockAuthenticationHandlerBase
    where TEntity : IGenericEntity
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder
            .ConfigureTestServices(services =>
            {
                services.RemoveAll<IMongoClient>();
                services.AddSingleton<IMongoClient>((_) => fixture.Client);
                fixture.SeedData(databaseName, collecttionName, ids).GetAwaiter().GetResult();
            });
    }
}
