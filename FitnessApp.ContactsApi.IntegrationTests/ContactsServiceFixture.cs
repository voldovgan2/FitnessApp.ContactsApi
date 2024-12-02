using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Common.Helpers;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models;
using FitnessApp.ContactsApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

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
        var categoryChangeHandler = new CategoryChangeHandler(storage);
        ContactsService = new ContactsService(
            storage,
            categoryChangeHandler,
            dateTimeService);

        SeedData().GetAwaiter().GetResult();
    }

    private async Task SeedData()
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

        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");

        var startuper0 = userRecords.Single(feh => feh.FirstName == "Fedir" && feh.LastName == "Nedashkovskiy");
        var startuper1 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nydoshkivskiy");
        var startuper2 = userRecords.Single(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        var startuper3 = userRecords.Single(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        var startuper4 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshok");
        var startuper5 = userRecords.Single(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        var startuper6 = userRecords.Single(feh => feh.FirstName == "Pedtse" && feh.LastName == "Nedoshok");
        var startuper7 = userRecords.Single(feh => feh.FirstName == "Hfedir" && feh.LastName == "Nedoshok");
        var startuper8 = userRecords.Single(feh => feh.FirstName == "Hfehin" && feh.LastName == "Nedoshok");
        var startuper9 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshko");

        await ContactsService.FollowUser(startuper0.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper1.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper2.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper3.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper4.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper5.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper6.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper7.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper8.UserId, sava.UserId);
        await ContactsService.FollowUser(startuper9.UserId, sava.UserId);
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

    public static async Task<T[]> GetRecords<T>(string collectionName)
        where T : IWithUserIdEntity
    {
        var client = new MongoClient("mongodb://127.0.0.1:27017");
        IMongoDatabase database = client.GetDatabase("FitnessContacts");
        var collection = database.GetCollection<T>(collectionName);
        var items = await DbContextHelper.FilterCollection(
            collection,
            FilterDefinition<T>.Empty);
        return items;
    }

    public static async Task<T[]> GetRecords<T>(string userId, string collectionName)
        where T : IWithUserIdEntity
    {
        var client = new MongoClient("mongodb://127.0.0.1:27017");
        IMongoDatabase database = client.GetDatabase("FitnessContacts");
        var collection = database.GetCollection<T>(collectionName);
        var items = await DbContextHelper.FilterCollection(
            collection,
            DbContextHelper.CreateGetByUserIdFiter<T>(userId));
        return items;
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
