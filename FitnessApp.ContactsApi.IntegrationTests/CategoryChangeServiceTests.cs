using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Helpers;
using MongoDB.Driver;
using DbContextHelper = FitnessApp.Common.Helpers.DbContextHelper;

namespace FitnessApp.ContactsApi.IntegrationTests;
public class CategoryChangeServiceTests(ContactsServiceFixture fixture) : IClassFixture<ContactsServiceFixture>
{
    [Fact]
    public async Task Test()
    {
        await StartupersFollowSava();
        await AfterPehiniovaFollowsSava_SavaUpgradesCategory();
        await AfterPehiniovaUnFollowsSava_SavaDowngradesCategory();
        await AfterSavaFollowsPehiniovaAndSavaChangeData_PehiniovaHasYoungSavaAsFollower();
    }

    private async Task StartupersFollowSava()
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

        await fixture.ContactsService.FollowUser(startuper0.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper1.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper2.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper3.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper4.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper5.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper6.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper7.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper8.UserId, sava.UserId);
        await fixture.ContactsService.FollowUser(startuper9.UserId, sava.UserId);
    }

    private async Task AfterPehiniovaFollowsSava_SavaUpgradesCategory()
    {
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");
        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Pehiniova");
        await fixture.ContactsService.FollowUser(pehiniova.UserId, sava.UserId);

        await fixture.CtegoryChangeHandler.Handle(new Contacts.Common.Events.CategoryChangedEvent
        {
            UserId = sava.UserId,
            OldCategory = 1,
            NewCategory = 2,
        });

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

    private async Task AfterPehiniovaUnFollowsSava_SavaDowngradesCategory()
    {
        var userRecords = await GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");
        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Pehiniova");
        await fixture.ContactsService.UnFollowUser(pehiniova.UserId, sava.UserId);
        await fixture.CtegoryChangeHandler.Handle(new Contacts.Common.Events.CategoryChangedEvent
        {
            UserId = sava.UserId,
            OldCategory = 2,
            NewCategory = 1,
        });

        userRecords = await GetRecords<UserEntity>("User");
        sava = userRecords.Single(u => u.LastName == "Sava");

        var firstCharRecords = await GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
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

        await fixture.ContactsService.FollowUser(sava.UserId, pehiniova.UserId);
        var oldSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        var youngSava = (await GetRecords<UserEntity>(sava.UserId, "User")).Single();
        youngSava.FirstName = "Roig";
        youngSava.LastName = "Vasa";
        await fixture.ContactsService.UpdateUser(oldSava, youngSava);
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

        ValidateLastNameFirstCharContextWithDefaultPartitionKey([sava], firstCharRecords);
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "r");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v", "FirstChar");
    }

    private static async Task<T[]> GetRecords<T>(string collectionName)
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

    private static async Task<T[]> GetRecords<T>(string userId, string collectionName)
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
