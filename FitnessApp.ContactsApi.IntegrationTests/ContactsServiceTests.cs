using System.Collections.Concurrent;
using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Common.Serializer;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Helpers;
using FitnessApp.Contacts.Common.Models;
using FitnessApp.Contacts.Common.Services;
using FitnessApp.ContactsApi.Services;
using FitnessApp.ContactsCategoryHandler;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using NATS.Client;
using DbContextHelper = FitnessApp.Common.Helpers.DbContextHelper;

namespace FitnessApp.ContactsApi.IntegrationTests;

public class ContactsServiceTests
{
    private class OptionsSnapshot(IConfiguration configuration) : IOptionsSnapshot<MongoDbSettings>
    {
        public MongoDbSettings Value => throw new NotImplementedException();

        public MongoDbSettings Get(string? name)
        {
            return new MongoDbSettings
            {
                DatabaseName = "FitnessContacts",
                CollecttionName = name,
                ConnectionString = configuration["MongoConnectionString"]
            };
        }
    }

    public readonly ContactsService _contactsService;
    public readonly CategoryChangeHandler _categoryChangeHandler;
    private readonly BlockingCollection<CategoryChangedEvent> _messageQueue = [];
    private readonly DateTimeService _dateTimeService = new();

    public ContactsServiceTests()
    {
        var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables();
        var configuration = builder.Build();
        var env = configuration["MongoConnectionString"];
        Console.WriteLine(env);
        _dateTimeService = new DateTimeService();
        var optionsSnapshot = new OptionsSnapshot(configuration);
        var client = CreateMongoClient();
        client.DropDatabase("FitnessContacts");
        var userDbContext = new UserDbContext(client, optionsSnapshot);
        var followerDbContext = new FollowerDbContext(client, optionsSnapshot);
        var followingDbContext = new FollowingDbContext(client, optionsSnapshot);
        var followerRequestDbContext = new FollowerRequestDbContext(
            client,
            optionsSnapshot,
            _dateTimeService);
        var lastNameFirstCharContext = new FirstCharSearchUserDbContext(client, optionsSnapshot);
        var firstCharsContext = new FirstCharSearchUserDbContext(client, optionsSnapshot);
        var firstCharMapContext = new FirstCharDbContext(client, optionsSnapshot);
        var followersContainer = new FollowersContainer(
            lastNameFirstCharContext,
            firstCharsContext,
            firstCharMapContext);
        var globalContainer = new GlobalContainer(firstCharsContext);
        var contactsRepository = new ContactsRepository(
            client,
            userDbContext,
            followerDbContext,
            followingDbContext,
            followerRequestDbContext,
            followersContainer,
            globalContainer);
        var usersCache = new UsersCache(new Mock<IDistributedCache>().Object);
        var storage = new Storage(
            usersCache,
            contactsRepository,
            _dateTimeService);
        var connectionFactory = new ConnectionFactory();
        connectionFactory.CreateConnection().SubscribeAsync(CategoryChangedEvent.Topic, (sender, args) =>
        {
            var receivedMessage = JsonConvertHelper.DeserializeFromBytes<CategoryChangedEvent>(args.Message.Data);
            _messageQueue.Add(receivedMessage);
        });
        var serviceBus = new ServiceBus(connectionFactory, configuration["NatsConnectionString"]);
        _contactsService = new ContactsService(
            storage,
            serviceBus,
            _dateTimeService);
        _categoryChangeHandler = new CategoryChangeHandler(storage);
        CreateUsers().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Test()
    {
        await AfterStartupersFollowSava_SavaHasStartupersAsFollowers();
        await AfterPehiniovaFollowsSava_SavaUpgradesCategory();
        await AfterPehiniovaChangesData_SavaHasNewPehiniova();
        await AfterPehiniovaUnFollowsSava_SavaDowngradesCategory();
        await AfterSavaFollowsPehiniovaAndSavaChangeData_PehiniovaHasYoungSavaAsFollower();
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

        await CreateUser("Myroslava", "Zubchyk");
    }

    private async Task CreateUser(string firstName, string lastName)
    {
        await _contactsService.AddUser(new UserModel
        {
            UserId = Guid.NewGuid().ToString("N"),
            FirstName = firstName,
            LastName = lastName,
        });
    }

    private async Task AfterStartupersFollowSava_SavaHasStartupersAsFollowers()
    {
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

        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper0.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper1.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper2.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper3.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper4.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper5.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper6.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper7.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper8.UserId, sava.UserId);
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(startuper9.UserId, sava.UserId);

        userRecords = await GetRecords<UserEntity>("User");
        sava = userRecords.Single(u => u.LastName == "Sava");

        await ValidateSavaFollowersAndFollowings(userRecords, sava.UserId);

        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Zubchyk");
        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        ValidateFirstChar(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharMapContextRecords,
            sava.UserId,
            sava.Category);

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");
        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            null);

        var savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(1, sava.Category);
    }

    private async Task AfterPehiniovaFollowsSava_SavaUpgradesCategory()
    {
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");
        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Zubchyk");
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(pehiniova.UserId, sava.UserId);

        var message = _messageQueue.Take();
        await _categoryChangeHandler.Handle(message);

        var savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(2, sava.Category);

        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        ValidateFirstChar(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharMapContextRecords,
            sava.UserId,
            sava.Category);

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");
        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            null);
    }

    private async Task AfterPehiniovaChangesData_SavaHasNewPehiniova()
    {
        var oldPehiniova = (await GetRecords<UserEntity>("User")).Single(p => p.FirstName == "Myroslava" && p.LastName == "Zubchyk");
        var youngPehiniova = (await GetRecords<UserEntity>("User")).Single(p => p.FirstName == "Myroslava" && p.LastName == "Zubchyk");
        youngPehiniova.FirstName = "Slava";
        youngPehiniova.LastName = "Niova";
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.UpdateUser(oldPehiniova, youngPehiniova);
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");

        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "z", FirstCharsEntityType.LastName);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "m", FirstCharsEntityType.FirstChars);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "my", FirstCharsEntityType.FirstChars);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "z", FirstCharsEntityType.FirstChars);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "zu", FirstCharsEntityType.FirstChars);

        ValidateFirstChar(
            [.. userRecords.Where(u => u.UserId != sava.UserId)],
            firstCharMapContextRecords,
            sava.UserId,
            sava.Category);

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");
        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            null);

        oldPehiniova = userRecords.Single(p => p.FirstName == "Slava" && p.LastName == "Niova");
        youngPehiniova = (await GetRecords<UserEntity>("User")).Single(p => p.FirstName == "Slava" && p.LastName == "Niova");
        youngPehiniova.FirstName = "Myroslava";
        youngPehiniova.LastName = "Pehiniova";
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.UpdateUser(oldPehiniova, youngPehiniova);
    }

    private async Task AfterPehiniovaUnFollowsSava_SavaDowngradesCategory()
    {
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");
        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Pehiniova");
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.UnFollowUser(pehiniova.UserId, sava.UserId);
        var pehinioviy = userRecords.Single(p => p.FirstName == "Fedir" && p.LastName == "Nedashkovskiy");
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.UnFollowUser(pehinioviy.UserId, sava.UserId);

        var savaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == sava.UserId);
        Assert.Null(savaFollowers.FirstOrDefault(sf => sf.FollowerId == pehiniova.UserId));

        var followings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == sava.UserId);
        Assert.Null(followings.FirstOrDefault(sf => sf.FollowingId == pehiniova.UserId));

        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");

        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "p", FirstCharsEntityType.LastName);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "m", FirstCharsEntityType.FirstChars);

        var message = _messageQueue.Take();
        await _categoryChangeHandler.Handle(message);

        userRecords = await GetRecords<UserEntity>("User");
        sava = userRecords.Single(u => u.LastName == "Sava");

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId && u.UserId != pehinioviy.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            sava.Category + 1);

        var savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(1, sava.Category);
    }

    private async Task AfterSavaFollowsPehiniovaAndSavaChangeData_PehiniovaHasYoungSavaAsFollower()
    {
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");

        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Pehiniova");

        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.FollowUser(sava.UserId, pehiniova.UserId);
        var oldSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        var youngSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        youngSava.FirstName = "Roig";
        youngSava.LastName = "Vasa";
        _dateTimeService.Now = DateTime.UtcNow;
        await _contactsService.UpdateUser(oldSava, youngSava);
        var savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();

        var pehiniovaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowers.Count(pf => pf.UserId == sava.UserId));
        var pehiniovaFollowings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowings.Count(pf => pf.FollowingId == sava.UserId));

        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.LastName);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "r", FirstCharsEntityType.FirstChars);

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "s");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "i");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "s", "FirstChar");

        ValidateLastNameFirstCharContextWithDefaultPartitionKey([sava], firstCharRecords);
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "r");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v", "FirstChar");
    }

    private static async Task<T[]> GetRecords<T>(string collectionName)
        where T : IWithUserIdEntity
    {
        var collection = GetCollection<T>(collectionName);
        return await DbContextHelper.FilterCollection(
            collection,
            FilterDefinition<T>.Empty);
    }

    private static async Task<T[]> GetRecords<T>(string userId, string collectionName)
        where T : IWithUserIdEntity
    {
        var collection = GetCollection<T>(collectionName);
        return await DbContextHelper.FilterCollection(
            collection,
            DbContextHelper.CreateGetByUserIdFiter<T>(userId));
    }

    private static IMongoCollection<T> GetCollection<T>(string collectionName)
        where T : IWithUserIdEntity
    {
        var client = CreateMongoClient();
        IMongoDatabase database = client.GetDatabase("FitnessContacts");
        return database.GetCollection<T>(collectionName);
    }

    private static MongoClient CreateMongoClient()
    {
        return new MongoClient("mongodb://127.0.0.1:27017");
    }

    private static async Task ValidateSavaFollowersAndFollowings(UserEntity[] userRecords, string savaUserId)
    {
        var savaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == savaUserId);
        var joinedFollowers = userRecords.Join(savaFollowers, ur => ur.UserId, sf => sf.UserId, (sf, ur) => new { sf, ur });
        Assert.Equal(savaFollowers.Count(), joinedFollowers.Count());
        var followings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == savaUserId);
        var joinedFollowings = userRecords.Join(followings, ur => ur.UserId, uf => uf.FollowingId, (sf, ur) => new { sf, ur });
        Assert.Equal(joinedFollowers.Count(), joinedFollowings.Count());
    }

    private static void ValidateFirstChar(
        UserEntity[] users,
        FirstCharEntity[] firstCharMapContextRecords,
        string savaUserId,
        int category)
    {
        HashSet<string> lastNameFirstChars = [];
        foreach (var user in users)
        {
            lastNameFirstChars.Add(KeyHelper.CreateKeyByChars(user.LastName[..1]));
        }

        foreach (var lastNameFirstChar in lastNameFirstChars)
        {
            EnsureInFirstCharContext(firstCharMapContextRecords, savaUserId, lastNameFirstChar, FirstCharsEntityType.LastName);
        }

        var firstCharFirstChars = GetFirstChars(users, category);
        foreach (var firstCharFirstChar in firstCharFirstChars)
        {
            EnsureInFirstCharContext(firstCharMapContextRecords, savaUserId, firstCharFirstChar, FirstCharsEntityType.FirstChars);
        }
    }

    private static void ValidateLastNameFirstCharContextWithDefaultPartitionKey(UserEntity[] users, FirstCharSearchUserEntity[] firstCharRecords)
    {
        foreach (var user in users)
        {
            var firstChars = GetFirstChars([user], 2);
            foreach (var firstChar in firstChars)
            {
                EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, user.UserId, firstChar);
            }
        }
    }

    private static void ValidateLastNameFirstCharContextWithCustomPartitionKey(
        UserEntity[] users,
        FirstCharSearchUserEntity[] firstCharRecords,
        string savaUserId,
        int minCategory,
        int? maxCategory)
    {
        var category = maxCategory ?? minCategory;
        foreach (var user in users)
        {
            var firstChars = GetFirstChars([user], category);
            var ensureInContextChars = firstChars.Where(firstChar => firstChar.Length <= minCategory);
            var ensureNotInContextChars = firstChars.Where(firstChar => firstChar.Length > minCategory);
            foreach (var firstChar in ensureInContextChars)
            {
                EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, savaUserId, user.UserId, firstChar);
            }

            foreach (var firstChar in ensureNotInContextChars)
            {
                EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, savaUserId, user.UserId, firstChar);
            }
        }
    }

    private static HashSet<string> GetFirstChars(UserEntity[] users, int category)
    {
        HashSet<string> firstCharFirstChars = [];
        for (int k = 1; k <= category; k++)
        {
            foreach (var user in users)
            {
                firstCharFirstChars.Add(KeyHelper.CreateKeyByChars(user.FirstName[..k]));
                firstCharFirstChars.Add(KeyHelper.CreateKeyByChars(user.LastName[..k]));
            }
        }

        return firstCharFirstChars;
    }

    private static void EnsureInFirstCharContext(
        FirstCharEntity[] records,
        string userId,
        string firstChars,
        FirstCharsEntityType firstCharsEntityType)
    {
        var filtered = records.Where(r => r.UserId == userId && r.FirstChars == firstChars && r.EntityType == firstCharsEntityType);
        Assert.Single(filtered);
    }

    private static void EnsureNotInFirstCharContext(
        FirstCharEntity[] records,
        string userId,
        string firstChars,
        FirstCharsEntityType firstCharsEntityType)
    {
        var filtered = records.Where(r => r.UserId == userId && r.FirstChars == firstChars && r.EntityType == firstCharsEntityType);
        Assert.Empty(filtered);
    }

    private static void EnsureLastNameFirstCharContextWithDefaultPartitionKey(
        FirstCharSearchUserEntity[] records,
        string userId,
        string firstChars)
    {
        var filtered = records.Where(r => r.UserId == userId && r.FirstChars == firstChars && r.PartitionKey == KeyHelper.CreateKeyByChars(firstChars));
        Assert.Single(filtered);
    }

    private static void EnsureInLastNameFirstCharContextWithCustomPartitionKey(
        FirstCharSearchUserEntity[] records,
        string contextUserId,
        string userId,
        string firstChars,
        string suffix = "")
    {
        var filtered = records.Where(r =>
            r.UserId == userId
            && r.FirstChars == firstChars
            && r.PartitionKey == KeyHelper.CreateKeyByChars(contextUserId, firstChars, suffix));
        Assert.Single(filtered);
    }

    private static void EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(
        FirstCharSearchUserEntity[] records,
        string contextUserId,
        string userId,
        string firstChars,
        string suffix = "")
    {
        var filtered = records.Where(r =>
            r.UserId == userId
            && r.FirstChars == firstChars
            && r.PartitionKey == KeyHelper.CreateKeyByChars(contextUserId, firstChars, suffix));
        Assert.Empty(filtered);
    }
}
