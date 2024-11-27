using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Exceptions;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public class FollowersContainer(
        IFirstCharSearchUserDbContext lastNameFirstCharContext,
        IFirstCharSearchUserDbContext firstCharsContext,
        IFirstCharDbContext firstCharMapContext) :
    ContainerBase(firstCharsContext),
    IFollowersContainer
{
    private const string _suffix = "FirstChar";

    public async Task AddUser(UserEntity user, UserEntity whoFollows)
    {
        var addUserToFirstCharsContextTask = AddUserToFirstCharsContext(
            whoFollows,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var addUserToLastNameFirstCharCollectionTask = AddUserToLastNameFirstCharCollection(user, whoFollows);
        var updateFirstCharsMapContextTask = UpdateFirstCharsContext(
            user,
            whoFollows,
            true);
        await Task.WhenAll(
            addUserToFirstCharsContextTask,
            addUserToLastNameFirstCharCollectionTask,
            updateFirstCharsMapContextTask);
    }

    public async Task UpdateUser(UserEntity user, UserEntity userToUpdate)
    {
        var updateUserInFirstCharsContextTask = UpdateUserInFirstCharsContext(
            user,
            (keys) => KeyHelper.CreateKeyByChars(userToUpdate.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(userToUpdate.Category));
        var updateUserInLastNameFirstCharCollectionTask = UpdateUserInLastNameFirstCharCollection(user, userToUpdate);
        await Task.WhenAll(
            updateUserInFirstCharsContextTask,
            updateUserInLastNameFirstCharCollectionTask);
    }

    public async Task RemoveUser(UserEntity user, UserEntity whoUnFollows)
    {
        var removeUserFromFirstCharsContextTask = RemoveUserFromFirstCharsContext(
            whoUnFollows,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var deleteUserfromLastNameFirstCharCollectionTask = DeleteUserfromLastNameFirstCharCollection(user, whoUnFollows);
        var updateFirstCharsToMapContextTask = UpdateFirstCharsContext(
            user,
            whoUnFollows,
            false);
        await Task.WhenAll(
            removeUserFromFirstCharsContextTask,
            deleteUserfromLastNameFirstCharCollectionTask,
            updateFirstCharsToMapContextTask);
    }

    public async Task UpdateUser(UserEntity user, UserEntity oldUser, UserEntity newUser)
    {
        var updateUserInFirstCharsContextTask = UpdateUserInFirstCharsContext(
            oldUser,
            newUser,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var updateUserByFirstCharTask = UpdateUserByFirstChar(user, oldUser, newUser);
        await Task.WhenAll(updateUserInFirstCharsContextTask, updateUserByFirstCharTask);
    }

    public async Task HandleCategoryChange(UserEntity user, CategoryChangedEvent categoryChangedEvent)
    {
        if (Math.Abs(categoryChangedEvent.OldCategory - categoryChangedEvent.NewCategory) != 1)
            throw new FollowersContainerException($"New category({categoryChangedEvent.NewCategory}) doesn't match to expected by old category({categoryChangedEvent.OldCategory})");
        var oldCharsCount = CategoryHelper.GetCategoryCharsCount(categoryChangedEvent.OldCategory);
        var newCharsCount = CategoryHelper.GetCategoryCharsCount(categoryChangedEvent.NewCategory);
        var isUpgrating = categoryChangedEvent.OldCategory < categoryChangedEvent.NewCategory;
        if (isUpgrating)
            await HandleUpgrade(user, oldCharsCount, newCharsCount);
        else
            await HandleDowngrade(user, newCharsCount);
    }

    public async Task<PagedDataModel<SearchUserEntity>> GetUsers(UserEntity user, GetUsersModel model)
    {
        return await GetUserFromFirstCharsContext(
            model,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
    }

    private async Task AddUserToLastNameFirstCharCollection(UserEntity user, UserEntity userToAdd)
    {
        var lastNameNormalized = KeyHelper.CreateKeyByChars(userToAdd.LastName[..1]);
        var firstCharSearchUser = ConvertHelper.FirstCharSearchUserEntityFromUserEntity(
            userToAdd,
            lastNameNormalized,
            KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix));
        await lastNameFirstCharContext.Add(firstCharSearchUser);
    }

    /// <summary>
    /// Updates user general info(for now just artificial rating) in search context(last name first chars), not affects partition key and chars count.
    /// </summary>
    /// <param name="user">User in wich context(partition key) to update.</param>
    /// <param name="userToUpdate">Who to update.</param>
    /// <returns>Task.</returns>
    private async Task UpdateUserInLastNameFirstCharCollection(UserEntity user, UserEntity userToUpdate)
    {
        var lastNameNormalized = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix);
        var firstCharSearchUser = await lastNameFirstCharContext.Get(new PartitionKeyAndIdAndFirstCharFilter(partitionKey, userToUpdate.UserId, lastNameNormalized));
        ConvertHelper.UpdateFirstCharSearchUserEntityByUserEntity(firstCharSearchUser, userToUpdate);
        await lastNameFirstCharContext.Update(firstCharSearchUser);
    }

    private async Task DeleteUserfromLastNameFirstCharCollection(UserEntity user, UserEntity userToRemove)
    {
        var key = KeyHelper.CreateKeyByChars(userToRemove.LastName[..1]);
        var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, key, _suffix);
        await lastNameFirstCharContext.Delete(new PartitionKeyAndIdAndFirstCharFilter(partitionKey, userToRemove.UserId, key));
    }

    private async Task UpdateUserByFirstChar(UserEntity user, UserEntity oldUser, UserEntity newUser)
    {
        if (oldUser.LastName.First() != newUser.LastName.First())
        {
            var deleteUserfromLastNameFirstCharCollectionTask = DeleteUserfromLastNameFirstCharCollection(user, oldUser);
            var addUserToLastNameFirstCharCollectionTask = AddUserToLastNameFirstCharCollection(user, newUser);
            var deleteFirstCharsContextTask = UpdateFirstCharsContext(user, oldUser, false);
            var addFirstCharsContextTask = UpdateFirstCharsContext(user, newUser, true);
            await Task.WhenAll(
                deleteUserfromLastNameFirstCharCollectionTask,
                addUserToLastNameFirstCharCollectionTask,
                deleteFirstCharsContextTask,
                addFirstCharsContextTask);
        }
    }

    private async Task HandleUpgrade(
        UserEntity user,
        int oldCharsCount,
        int newCharsCount)
    {
        var followers = await GetFlatFollowers(user.UserId);
        await UpgradeFirstCharsContext(user, followers, oldCharsCount, newCharsCount);
        await UpgradeFirstCharsMapContext(user, followers, oldCharsCount, newCharsCount);
    }

    private async Task UpgradeFirstCharsContext(
        UserEntity user,
        FirstCharSearchUserEntity[] followers,
        int oldCharsCount,
        int newCharsCount)
    {
        var users = followers.
            Select(follower =>
            {
                var followersByChars = KeyHelper.GetKeysByFirstChars(
                    follower,
                    oldCharsCount,
                    newCharsCount);
                return followersByChars.Select(followerByChars =>
                {
                    return ConvertHelper.FirstCharSearchUserEntityFromFirstCharSearchUserEntity(
                        follower,
                        followerByChars,
                        KeyHelper.CreateKeyByChars(user.UserId, followerByChars));
                });
            }).SelectMany(users => users);
        await FirstCharsContext.Add([.. users]);
    }

    private async Task UpgradeFirstCharsMapContext(
        UserEntity user,
        SearchUserEntity[] followers,
        int oldCharsCount,
        int newCharsCount)
    {
        var followerByChars = followers.
            Select(follower =>
            {
                return KeyHelper.GetKeysByFirstChars(
                    follower,
                    oldCharsCount,
                    newCharsCount);
            }).SelectMany(users => users);
        foreach (var followerByChar in followerByChars)
        {
            await UpdateFirstCharsContext(
                        user,
                        FirstCharsEntityType.FirstChars,
                        followerByChar,
                        true);
        }
    }

    private async Task HandleDowngrade(UserEntity user, int newCharsCount)
    {
        var @params = await CreatePartitionKeyAndFirstCharParamsFromFirstCharsValue(
            FirstCharsEntityType.FirstChars,
            firstCharsValue => firstCharsValue.FirstChars.Length > newCharsCount,
            user.UserId);
        await FirstCharsContext.Delete([..@params]);
    }

    private async Task UpdateFirstCharsContext(
        UserEntity user,
        UserEntity userToUpdate,
        bool increase)
    {
        var updateFirstCharsContextTasks = new List<Task>();
        var lastNameFirstChar = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        updateFirstCharsContextTasks.Add(UpdateFirstCharsContext(user, FirstCharsEntityType.LastName, lastNameFirstChar, increase));
        var charsCount = CategoryHelper.GetCategoryCharsCount(user.Category);
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToUpdate, 0, charsCount);
        updateFirstCharsContextTasks.AddRange(firstCharsKeys.Select(firstCharsKey => UpdateFirstCharsContext(user, FirstCharsEntityType.FirstChars, firstCharsKey, increase)));
        foreach (var task in updateFirstCharsContextTasks)
        {
            await task;
        }
    }

    private async Task UpdateFirstCharsContext(
        UserEntity user,
        FirstCharsEntityType firstCharsEntityType,
        string lastNameFirstChar,
        bool increase)
    {
        var firstCharEntity = await firstCharMapContext.TryGet(user.UserId, lastNameFirstChar, firstCharsEntityType);
        if (firstCharEntity == null)
        {
            if (!increase)
            {
                throw new FirstCharEntityNotFoundException(user.UserId, lastNameFirstChar, firstCharsEntityType);
            }
            else
            {
                await firstCharMapContext.Add(new FirstCharEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = user.UserId,
                    EntityType = firstCharsEntityType,
                    FirstChars = lastNameFirstChar,
                    FollowersCount = 1,
                });
            }
        }
        else
        {
            int delta = increase ? 1 : -1;
            firstCharEntity.FollowersCount += delta;
            if (firstCharEntity.FollowersCount == 0)
            {
                await firstCharMapContext.Delete(user.UserId, lastNameFirstChar, firstCharsEntityType);
            }
            else
            {
                await firstCharMapContext.Update(firstCharEntity);
            }
        }
    }

    private async Task<FirstCharSearchUserEntity[]> GetFlatFollowers(string userId)
    {
        var @params = await CreatePartitionKeyAndFirstCharParamsFromFirstCharsValue(
            FirstCharsEntityType.LastName,
            o => true,
            userId);
        return [
            .. (
                    await Task.WhenAll(@params.Select(lastNameFirstCharContext.Get))
                ).SelectMany(items => items)
        ];
    }

    private async Task<IEnumerable<PartitionKeyAndFirstCharFilter>> CreatePartitionKeyAndFirstCharParamsFromFirstCharsValue(
        FirstCharsEntityType entityType,
        Func<FirstCharEntity, bool> predicate,
        string userId)
    {
        var firstCharsValues = await firstCharMapContext.Get(userId, entityType);
        var filtered = firstCharsValues.Where(item => predicate(item));
        return filtered.Select(firstCharsValue =>
        {
            var partitionKey = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars, _suffix);
            return new PartitionKeyAndFirstCharFilter(partitionKey, firstCharsValue.FirstChars);
        });
    }
}
