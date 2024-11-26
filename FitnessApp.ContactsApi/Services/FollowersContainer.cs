using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Exceptions;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public class FollowersContainer(
        IMapper mapper,
        IFirstCharSearchUserDbContext lastNameFirstCharContext,
        IFirstCharSearchUserDbContext firstCharsContext,
        IFirstCharDbContext firstCharMapContext) :
    ContainerBase(mapper, firstCharsContext),
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
        await UpdateUserInFirstCharsContext(
            oldUser,
            newUser,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        await UpdateUserByFirstChar(user, oldUser, newUser);
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
        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToAdd);
        firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix);
        firstCharSearchUser.FirstChars = lastNameNormalized;
        await lastNameFirstCharContext.Add(firstCharSearchUser);
    }

    private async Task UpdateUserInLastNameFirstCharCollection(UserEntity user, UserEntity userToUpdate)
    {
        var lastNameNormalized = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToUpdate);
        firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix);
        firstCharSearchUser.FirstChars = lastNameNormalized;
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

    private async Task HandleUpgrade(UserEntity user, int oldCharsCount, int newCharsCount)
    {
        var followers = await GetFlatFollowers(user.UserId);
        var users = followers.
            Select(follower =>
            {
                var followersByChars = KeyHelper.GetKeysByFirstChars(
                    follower,
                    oldCharsCount,
                    newCharsCount);
                return followersByChars.Select(followerByChars =>
                {
                    if (oldCharsCount < 0)
                    {
                        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(follower);
                        firstCharSearchUser.Id = Guid.NewGuid().ToString("N");
                        firstCharSearchUser.FirstChars = followerByChars;
                        firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, followerByChars);
                        return firstCharSearchUser;
                    }

                    return new FirstCharSearchUserEntity
                    {
                        UserId = follower.UserId,
                        Category = follower.Category,
                        FirstName = follower.FirstName,
                        LastName = follower.LastName,
                        Rating = follower.Rating,
                        Id = Guid.NewGuid().ToString("N"),
                        FirstChars = followerByChars,
                        PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, followerByChars),
                    };
                });
            }).SelectMany(users => users);
        ArgumentNullException.ThrowIfNull(nameof(users));
        await FirstCharsContext.Add([..users]);
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
        var lastNameFirstChar = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        var firstCharEntity = await firstCharMapContext.TryGet(user.UserId, lastNameFirstChar, FirstCharsEntityType.LastName);
        if (firstCharEntity == null)
        {
            if (!increase)
            {
                throw new FirstCharEntityNotFoundException(user.UserId, lastNameFirstChar, FirstCharsEntityType.LastName);
            }
            else
            {
                await firstCharMapContext.Add(new FirstCharEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = user.UserId,
                    EntityType = FirstCharsEntityType.LastName,
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
                await firstCharMapContext.Delete(user.UserId, lastNameFirstChar, FirstCharsEntityType.LastName);
            }
            else
            {
                await firstCharMapContext.Update(firstCharEntity);
            }
        }
    }

    private async Task<SearchUserEntity[]> GetFlatFollowers(string userId)
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
            var partitionKey = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars);
            return new PartitionKeyAndFirstCharFilter(partitionKey, firstCharsValue.FirstChars);
        });
    }
}
