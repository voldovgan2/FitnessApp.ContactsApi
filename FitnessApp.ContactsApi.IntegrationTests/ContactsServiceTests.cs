using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Helpers;
using MongoDB.Driver;

namespace FitnessApp.ContactsApi.IntegrationTests;

public class ContactsServiceTests(ContactsServiceFixture fixture)
{
    [Fact]
    public async Task AfterPehiniovaUnFollowsSava_SavaUpgradesCategory()
    {
        var userRecords = await ContactsServiceFixture.GetRecords<UserEntity>("User");
        var sava = userRecords.Single(u => u.LastName == "Sava");
        Assert.NotNull(sava);

        var pehiniova = userRecords.Single(p => p.FirstName == "Myroslava" && p.LastName == "Pehiniova");
        await fixture.ContactsService.FollowUser(pehiniova.UserId, sava.UserId);

        var savaFollowers = (await ContactsServiceFixture.GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == sava.UserId);
        var joinedFollowers = userRecords.Join(savaFollowers, ur => ur.UserId, sf => sf.UserId, (sf, ur) => new { sf, ur });
        Assert.Equal(savaFollowers.Count(), joinedFollowers.Count());
        var followings = (await ContactsServiceFixture.GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == sava.UserId);
        var joinedFollowings = userRecords.Join(followings, ur => ur.UserId, uf => uf.FollowingId, (sf, ur) => new { sf, ur });
        Assert.Equal(joinedFollowers.Count(), joinedFollowings.Count());

        var firstCharMapContextRecords = await ContactsServiceFixture.GetRecords<FirstCharEntity>("FirstChar");
        ValidateFirstChar(
            [..userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharMapContextRecords,
            sava.UserId,
            sava.Category);

        var firstCharRecords = await ContactsServiceFixture.GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");
        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            null);

        var savaRecords = await ContactsServiceFixture.GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(2, sava.Category);

        await fixture.ContactsService.UnFollowUser(pehiniova.UserId, sava.UserId);

        savaFollowers = (await ContactsServiceFixture.GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == sava.UserId);
        Assert.Null(savaFollowers.FirstOrDefault(sf => sf.FollowerId == pehiniova.UserId));

        followings = (await ContactsServiceFixture.GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == sava.UserId);
        Assert.Null(followings.FirstOrDefault(sf => sf.FollowingId == pehiniova.UserId));

        firstCharMapContextRecords = await ContactsServiceFixture.GetRecords<FirstCharEntity>("FirstChar");

        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "p", FirstCharsEntityType.LastName);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "m", FirstCharsEntityType.FirstChars);
        EnsureNotInFirstCharContext(firstCharMapContextRecords, sava.UserId, "my", FirstCharsEntityType.FirstChars);

        firstCharRecords = await ContactsServiceFixture.GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        ValidateLastNameFirstCharContextWithDefaultPartitionKey(userRecords, firstCharRecords);
        ValidateLastNameFirstCharContextWithCustomPartitionKey(
            [.. userRecords.Where(u => u.UserId != sava.UserId && u.UserId != pehiniova.UserId)],
            firstCharRecords,
            sava.UserId,
            sava.Category,
            sava.Category + 1);

        savaRecords = await ContactsServiceFixture.GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();
        Assert.Equal(1, sava.Category);

        await fixture.ContactsService.FollowUser(sava.UserId, pehiniova.UserId);
        var oldSava = (await ContactsServiceFixture.GetRecords<UserEntity>(sava.UserId, "User")).Single();
        var youngSava = (await ContactsServiceFixture.GetRecords<UserEntity>(sava.UserId, "User")).Single();
        youngSava.FirstName = "Roig";
        youngSava.LastName = "Vasa";
        await fixture.ContactsService.UpdateUser(oldSava, youngSava);
        savaRecords = await ContactsServiceFixture.GetRecords<UserEntity>(sava.UserId, "User");
        sava = savaRecords.Single();

        var pehiniovaFollowers = (await ContactsServiceFixture.GetRecords<MyFollowerEntity>("Follower")).Where(f => f.FollowerId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowers.Count(pf => pf.UserId == sava.UserId));
        var pehiniovaFollowings = (await ContactsServiceFixture.GetRecords<MeFollowingEntity>("Following")).Where(f => f.UserId == pehiniova.UserId);
        Assert.Equal(1, pehiniovaFollowings.Count(pf => pf.FollowingId == sava.UserId));

        firstCharMapContextRecords = await ContactsServiceFixture.GetRecords<FirstCharEntity>("FirstChar");
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.LastName);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "v", FirstCharsEntityType.FirstChars);
        EnsureInFirstCharContext(firstCharMapContextRecords, pehiniova.UserId, "r", FirstCharsEntityType.FirstChars);

        firstCharRecords = await ContactsServiceFixture.GetRecords<FirstCharSearchUserEntity>("FirstCharSearchUser");

        ValidateLastNameFirstCharContextWithDefaultPartitionKey([sava], firstCharRecords);
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "r");
        EnsureInLastNameFirstCharContextWithCustomPartitionKey(firstCharRecords, pehiniova.UserId, sava.UserId, "v", "FirstChar");
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
            lastNameFirstChars.Add(user.LastName[..2]);
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
        var firstChars = GetFirstChars(users, 2);
        foreach (var user in users)
        {
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
        var firstChars = GetFirstChars(users, category);
        var ensureInContextChars = firstChars.Where(firstChar => firstChar.Length <= minCategory);
        var ensureNotInContextChars = firstChars.Where(firstChar => firstChar.Length > minCategory);
        foreach (var user in users)
        {
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
        for (int k = 1; k < category; k++)
        {
            foreach (var user in users)
            {
                firstCharFirstChars.Add(user.FirstName[..k]);
                firstCharFirstChars.Add(user.LastName[..k]);
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
