using FitnessApp.Common.Abstractions.Db;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Models;
using FitnessApp.ContactsApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

namespace FitnessApp.ContactsApi.IntegrationTests;

public class ContactsServiceTests
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

    private readonly ContactsService _contactsService;

    public ContactsServiceTests()
    {
        var dateTimeService = new DateTimeService();
        var optionsSnapshot = new OptionsSnapshot();
        var client = new MongoClient("mongodb://127.0.0.1:27017");
        var userDbContext = new UserDbContext(client, optionsSnapshot);
        var followerDbContext = new FollowerDbContext(client, optionsSnapshot);
        var followingDbContext = new FollowingDbContext(client, optionsSnapshot);
        var followerRequestDbContext = new FollowerRequestDbContext(
            client,
            optionsSnapshot,
            dateTimeService);
        var lastNameFirstCharContext = new FirstCharSearchUserDbContext(client, optionsSnapshot);
        var firstCharsContext = new FirstCharSearchUserDbContext(client, optionsSnapshot);
        var firstCharMapContext = new FirstCharDbContext(client, optionsSnapshot);
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
        _contactsService = new ContactsService(
            storage,
            categoryChangeHandler,
            dateTimeService);
    }

    [Fact]
    public async Task AfterPehiniovaUnFollowsSava_SavaUpgradesCategory()
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
        Assert.NotNull(sava);

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

        await _contactsService.FollowUser(startuper0.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper1.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper2.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper3.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper4.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper5.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper6.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper7.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper8.UserId, sava.UserId);
        await _contactsService.FollowUser(startuper9.UserId, sava.UserId);

        var pehiniova = userRecords.Single(feh => feh.FirstName == "Myroslava" && feh.LastName == "Pehiniova");
        await _contactsService.FollowUser(pehiniova.UserId, sava.UserId);

        var savaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == sava.UserId);
        var joinedFollowers = userRecords.Join(savaFollowers, ur => ur.UserId, sf => sf.UserId, (sf, ur) => new { sf, ur });
        Assert.Equal(savaFollowers.Count(), joinedFollowers.Count());
        var followings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == sava.UserId);
        var joinedFollowings = userRecords.Join(followings, ur => ur.UserId, uf => uf.FollowingId, (sf, ur) => new { sf, ur });
        Assert.Equal(joinedFollowers.Count(), joinedFollowings.Count());

        var firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "n", FirstCharsEntityType.LastName);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "p", FirstCharsEntityType.LastName);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "f", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "fe", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "p", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "pe", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "h", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "hf", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "m", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "my", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "n", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "ne", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, sava.UserId, "ny", FirstCharsEntityType.FirstChars);

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        // var startuper0 = userRecords.Single(feh => feh.FirstName == "Fedir" && feh.LastName == "Nedashkovskiy");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "f");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "n", "FirstChar");

        // var startuper1 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nydoshkivskiy");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "ny");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "ny");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "n", "FirstChar");

        // var startuper2 = userRecords.Single(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "f");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "n", "FirstChar");

        // var startuper3 = userRecords.Single(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "n", "FirstChar");

        // var startuper4 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "n", "FirstChar");

        // var startuper5 = userRecords.Single(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "f");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "n", "FirstChar");

        // var startuper6 = userRecords.Single(feh => feh.FirstName == "Pedtse" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "n", "FirstChar");

        // var startuper7 = userRecords.Single(feh => feh.FirstName == "Hfedir" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "h");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "hf");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "h");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "hf");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "n", "FirstChar");

        // var startuper8 = userRecords.Single(feh => feh.FirstName == "Hfehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "h");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "hf");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "h");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "hf");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "n", "FirstChar");

        // var startuper9 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshko");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "n");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "n", "FirstChar");

        // var pehiniova = userRecords.Single(feh => feh.FirstName == "Myroslava" && feh.LastName == "Pehiniova");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "m");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "my");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "m");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "my");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "p");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "p", "FirstChar");

        // var sava = userRecords.Single(u => u.LastName == "Sava");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "i");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "ig");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "s");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "sa");

        var savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(2, sava.Category);

        await _contactsService.UnFollowUser(pehiniova.UserId, sava.UserId);

        savaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == sava.UserId);
        Assert.Null(savaFollowers.FirstOrDefault(sf => sf.FollowerId == pehiniova.UserId));

        followings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == sava.UserId);
        Assert.Null(followings.FirstOrDefault(sf => sf.FollowingId == pehiniova.UserId));

        firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");

        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "p", FirstCharsEntityType.LastName);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "m", FirstCharsEntityType.FirstChars);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "my", FirstCharsEntityType.FirstChars);

        firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        // var startuper0 = userRecords.Single(feh => feh.FirstName == "Fedir" && feh.LastName == "Nedashkovskiy");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper0.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "f");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper0.UserId, "n", "FirstChar");

        // var startuper1 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nydoshkivskiy");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper1.UserId, "ny");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "ny");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper1.UserId, "n", "FirstChar");

        // var startuper2 = userRecords.Single(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper2.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "f");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper2.UserId, "n", "FirstChar");

        // var startuper3 = userRecords.Single(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper3.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper3.UserId, "n", "FirstChar");

        // var startuper4 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper4.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper4.UserId, "n", "FirstChar");

        // var startuper5 = userRecords.Single(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "f");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "fe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper5.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "f");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "fe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper5.UserId, "n", "FirstChar");

        // var startuper6 = userRecords.Single(feh => feh.FirstName == "Pedtse" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper6.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper6.UserId, "n", "FirstChar");

        // var startuper7 = userRecords.Single(feh => feh.FirstName == "Hfedir" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "h");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "hf");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper7.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "h");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "hf");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper7.UserId, "n", "FirstChar");

        // var startuper8 = userRecords.Single(feh => feh.FirstName == "Hfehin" && feh.LastName == "Nedoshok");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "h");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "hf");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper8.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "h");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "hf");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper8.UserId, "n", "FirstChar");

        // var startuper9 = userRecords.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshko");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "pe");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "n");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, startuper9.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "pe");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "n");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "ne");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, startuper9.UserId, "n", "FirstChar");

        // var pehiniova = userRecords.Single(feh => feh.FirstName == "Myroslava" && feh.LastName == "Pehiniova");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "m");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "my");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "p");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, pehiniova.UserId, "pe");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "m");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "my");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "p");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "pe");
        EnsureNotInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, sava.UserId, pehiniova.UserId, "p", "FirstChar");

        // var sava = userRecords.Single(u => u.LastName == "Sava");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "i");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "ig");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "s");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "sa");

        savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(1, sava.Category);

        await _contactsService.FollowUser(sava.UserId, pehiniova.UserId);
        var oldSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        var youngSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        youngSava.FirstName = "Roig";
        youngSava.LastName = "Vasa";
        await _contactsService.UpdateUser(oldSava, youngSava);
        savaRecords = await GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();

        var pehiniovaFollowers = (await GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowers.Count(pf => pf.UserId == sava.UserId));
        var pehiniovaFollowings = (await GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowings.Count(pf => pf.FollowingId == sava.UserId));

        firstCharMapContextRecords = await GetRecords<FirstCharEntity>("FirstChar");
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.LastName);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "r", FirstCharsEntityType.FirstChars);

        firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "v");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "va");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "r");
        EnsureLastNameFirstCharContextWithDefaultPartitionKey(firstCharRecords, sava.UserId, "ro");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "r");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v", "FirstChar");
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

    private static async Task<T[]> GetRecords<T>(string collectionName)
        where T : IWithUserIdEntity
    {
        var client = new MongoClient("mongodb://127.0.0.1:27017");
        IMongoDatabase database = client.GetDatabase("FitnessContacts");
        var collection = database.GetCollection<T>(collectionName);
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            collection,
            FilterDefinition<T>.Empty);
        return items;
    }

    private static async Task<T[]> GetRecords<T>(string userId, string collectionName)
        where T : IWithUserIdEntity
    {
        var client = new MongoClient("mongodb://127.0.0.1:27017");
        IMongoDatabase database = client.GetDatabase("FitnessContacts");
        var collection = database.GetCollection<T>(collectionName);
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            collection,
            DbContextHelper.CreateGetByUserIdFiter<T>(userId));
        return items;
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
