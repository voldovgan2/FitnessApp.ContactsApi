using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.Contacts.Common.Models;
using FitnessApp.Contacts.Common.Services;
using FitnessApp.ContactsApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using NATS.Client;

namespace FitnessApp.ContactsApi.IntegrationTests;
public class ContactsServiceFixture : IDisposable
{
    private class OptionsSnapshot : IOptionsSnapshot<MongoDbSettings>
    {
        public MongoDbSettings Value => throw new NotImplementedException();

        public MongoDbSettings Get(string? name)
        {
            return new MongoDbSettings
            {
                DatabaseName = "FitnessContacts",
                CollecttionName = name,
                ConnectionString = "mongodb://127.0.0.1:27017"
            };
        }
    }

    public readonly ContactsService ContactsService;
    private readonly MongoClient _client;

    public ContactsServiceFixture()
    {
        var dateTimeService = new DateTimeService();
        var optionsSnapshot = new OptionsSnapshot();
        _client = new MongoClient("mongodb://127.0.0.1:27017");
        var userDbContext = new UserDbContext(_client, optionsSnapshot);
        var followerDbContext = new FollowerDbContext(_client, optionsSnapshot);
        var followingDbContext = new FollowingDbContext(_client, optionsSnapshot);
        var followerRequestDbContext = new FollowerRequestDbContext(
            _client,
            optionsSnapshot,
            dateTimeService);
        var lastNameFirstCharContext = new FirstCharSearchUserDbContext(_client, optionsSnapshot);
        var firstCharsContext = new FirstCharSearchUserDbContext(_client, optionsSnapshot);
        var firstCharMapContext = new FirstCharDbContext(_client, optionsSnapshot);
        var followersContainer = new FollowersContainer(
            lastNameFirstCharContext,
            firstCharsContext,
            firstCharMapContext);
        var globalContainer = new GlobalContainer(firstCharsContext);
        var contactsRepository = new ContactsRepository(
            userDbContext,
            followerDbContext,
            followingDbContext,
            followerRequestDbContext,
            followersContainer,
            globalContainer);
        var usersCache = new UsersCache(new Mock<IDistributedCache>().Object);
        var storage = new Storage(usersCache, contactsRepository);
        var connectionFactory = new ConnectionFactory();
        var serviceBus = new ServiceBus(connectionFactory, "nats://127.0.0.1:4222");
        ContactsService = new ContactsService(
            storage,
            serviceBus,
            dateTimeService);

        CreateUsers().GetAwaiter().GetResult();
    }

    private async Task CreateUsers()
    {
        await CreateUser("Igor", "Sava");

        await CreateUser("Fedir", "Nedashkovskiy");
        await CreateUser("Pedir", "Nydoshkivskiy");

        await CreateUser("Fehin", "Nedoshok");
        await CreateUser("Pehin", "Nedoshok");
        await CreateUser("Pedir", "Nedoshok");

        await CreateUser("Fedtse", "Nedoshok");
        await CreateUser("Pedtse", "Nedoshok");

        await CreateUser("Hfedir", "Nedoshok");
        await CreateUser("Hfehin", "Nedoshok");

        await CreateUser("Pedir", "Nedoshko");

        await CreateUser("Myroslava", "Pehiniova");
    }

    private async Task CreateUser(string firstName, string lastName)
    {
        await ContactsService.AddUser(new UserModel
        {
            UserId = Guid.NewGuid().ToString("N"),
            FirstName = firstName,
            LastName = lastName,
        });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _client.DropDatabase("FitnessContacts");
    }
}
