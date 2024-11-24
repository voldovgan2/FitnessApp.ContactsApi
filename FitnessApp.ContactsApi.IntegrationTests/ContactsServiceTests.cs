using AutoMapper;
using FitnessApp.Common.Abstractions.Db;
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
        _contactsService = CreateContactsService();
    }

    [Fact]
    public async Task HfedFollowsSava_SavaHasHfedsAsFollowers()
    {
        await CreateUser("Igor", "Sava");
        await CreateUser("Fedir", "Nedashkovskiy");
        var sava = (await GetUsers("Sa")).Single();
        var fehin = (await GetUsers("Ne")).Single();
        await _contactsService.FollowUser(fehin.UserId, sava.UserId);
        var users = await GetSavaFollowers("f", sava.UserId);
        Assert.Single(users);
        Assert.Contains(users, user => user.UserId == fehin.UserId);
    }

    [Fact]
    public async Task HfedUnFollowsSava_SavaNotHasHfedsAsFollowers()
    {
        await CreateUser("Igor", "Sava");
        await CreateUser("Fedir", "Nedashkovskiy");
        var sava = (await GetUsers("Sa")).Single();
        var fehin = (await GetUsers("Ne")).Single();
        await _contactsService.FollowUser(fehin.UserId, sava.UserId);
        await _contactsService.UnFollowUser(fehin.UserId, sava.UserId);
        var users = await GetSavaFollowers("f", sava.UserId);
        Assert.Empty(users);
    }

    [Fact]
    public async Task AfterHfedFollowsSava_SavaUpgradesCategory()
    {
        CreateUsers();
        var sava = (await GetUsers("Sa")).Single();
        var allStartupers = await GetUsers("Ne");
        var startuper0 = allStartupers.Single(feh => feh.FirstName == "Fedir" && feh.LastName == "Nedashkovskiy");
        var startuper1 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedashkovskiy");
        var startuper2 = allStartupers.Single(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        var startuper3 = allStartupers.Single(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        var startuper4 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshok");
        var startuper5 = allStartupers.Single(feh => feh.FirstName == "Pedtse" && feh.LastName == "Nedoshok");
        var startuper6 = allStartupers.Single(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        var startuper7 = allStartupers.Single(feh => feh.FirstName == "Hfedir" && feh.LastName == "Nedoshok");
        var startuper8 = allStartupers.Single(feh => feh.FirstName == "Hfehin" && feh.LastName == "Nedoshok");
        var startuper9 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshko");

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

        var pehiniova = (await GetUsers("Pe")).Single(feh => feh.FirstName == "Myroslava" && feh.LastName == "Pehiniova");
        await _contactsService.FollowUser(pehiniova.UserId, sava.UserId);

        sava = (await GetUsers("Sa")).Single();

        var users = await GetSavaFollowers("f", sava.UserId);
        Assert.Equal(3, users.Count());
    }

    [Fact]
    public async Task AfterPehiniovaUnFollowsSava_SavaDowngradesCategory()
    {
        CreateUsers();
        var sava = (await GetUsers("Sa")).Single();
        var allStartupers = await GetUsers("Ne");
        var startuper0 = allStartupers.Single(feh => feh.FirstName == "Fedir" && feh.LastName == "Nedashkovskiy");
        var startuper1 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedashkovskiy");
        var startuper2 = allStartupers.Single(feh => feh.FirstName == "Fehin" && feh.LastName == "Nedoshok");
        var startuper3 = allStartupers.Single(feh => feh.FirstName == "Pehin" && feh.LastName == "Nedoshok");
        var startuper4 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshok");
        var startuper5 = allStartupers.Single(feh => feh.FirstName == "Pedtse" && feh.LastName == "Nedoshok");
        var startuper6 = allStartupers.Single(feh => feh.FirstName == "Fedtse" && feh.LastName == "Nedoshok");
        var startuper7 = allStartupers.Single(feh => feh.FirstName == "Hfedir" && feh.LastName == "Nedoshok");
        var startuper8 = allStartupers.Single(feh => feh.FirstName == "Hfehin" && feh.LastName == "Nedoshok");
        var startuper9 = allStartupers.Single(feh => feh.FirstName == "Pedir" && feh.LastName == "Nedoshko");

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

        var pehiniova = (await GetUsers("Pe")).Single(feh => feh.FirstName == "Myroslava" && feh.LastName == "Pehiniova");
        await _contactsService.FollowUser(pehiniova.UserId, sava.UserId);
        await _contactsService.UnFollowUser(pehiniova.UserId, sava.UserId);

        sava = (await GetUsers("Sa")).Single();

        var users = await GetSavaFollowers("f", sava.UserId);
        Assert.Equal(3, users.Count());
    }

    private void CreateUsers()
    {
        CreateUser("Igor", "Sava").GetAwaiter().GetResult();

        CreateUser("Fedir", "Nedashkovskiy").GetAwaiter().GetResult();
        CreateUser("Pedir", "Nedashkovskiy").GetAwaiter().GetResult();

        CreateUser("Fehin", "Nedoshok").GetAwaiter().GetResult();
        CreateUser("Pehin", "Nedoshok").GetAwaiter().GetResult();
        CreateUser("Pedir", "Nedoshok").GetAwaiter().GetResult();

        CreateUser("Pedtse", "Nedoshok").GetAwaiter().GetResult();
        CreateUser("Fedtse", "Nedoshok").GetAwaiter().GetResult();

        CreateUser("Hfedir", "Nedoshok").GetAwaiter().GetResult();
        CreateUser("Hfehin", "Nedoshok").GetAwaiter().GetResult();

        CreateUser("Pedir", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Fehin", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Pehin", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Pedtse", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Fedtse", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Hfedir", "Nedoshko").GetAwaiter().GetResult();
        CreateUser("Hfehin", "Nedoshko").GetAwaiter().GetResult();

        CreateUser("Myroslava", "Pehiniova").GetAwaiter().GetResult();
    }

    private static ContactsService CreateContactsService()
    {
        var dateTimeService = new DateTimeService();
        var mapperConfiguration = new MapperConfiguration(delegate(IMapperConfigurationExpression mc)
        {
            mc.AddProfile(new MappingProfile());
        });
        var mapper = mapperConfiguration.CreateMapper();
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
            mapper,
            lastNameFirstCharContext,
            firstCharsContext,
            firstCharMapContext);
        var globalContainer = new GlobalContainer(mapper, firstCharsContext);
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
        return new ContactsService(
            storage,
            categoryChangeHandler,
            mapper,
            dateTimeService);
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

    private async Task<UserModel[]> GetUsers(string search)
    {
        var model = CreateGetUsersModel(search);
        var users = await _contactsService.GetUsers(model);
        return [.. users.Items];
    }

    private async Task<UserModel[]> GetSavaFollowers(string search, string savaUserId)
    {
        var model = new GetUsersModel { Search = search, Page = 0, PageSize = 10 };
        var usersModel = await _contactsService.GetUserFollowers(savaUserId, model);
        return [.. usersModel.Items];
    }

    private static GetUsersModel CreateGetUsersModel(string search)
    {
        return new GetUsersModel { Search = search, Page = 0, PageSize = 100 };
    }
}
