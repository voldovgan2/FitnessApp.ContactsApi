using System;
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
        return await GetUserFromFirstCharsContext(model, CategoryHelper.GetCategoryCharsCount(user.Category));
    }

    private async Task AddUserToLastNameFirstCharCollection(UserEntity user, UserEntity userToAdd)
    {
        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToAdd);
        firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, userToAdd.LastName[..1], _suffix);
        firstCharSearchUser.FirstChars = userToAdd.LastName[..1];
        await lastNameFirstCharContext.Add(firstCharSearchUser);
    }

    private async Task UpdateUserInLastNameFirstCharCollection(UserEntity user, UserEntity userToUpdate)
    {
        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToUpdate);
        firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, userToUpdate.LastName[..1], _suffix);
        firstCharSearchUser.FirstChars = userToUpdate.LastName[..1];
        await lastNameFirstCharContext.Update(firstCharSearchUser);
    }

    private async Task DeleteUserfromLastNameFirstCharCollection(UserEntity user, UserEntity userToRemove)
    {
        var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, userToRemove.LastName[..1], _suffix);
        var key = userToRemove.LastName[..1];
        await lastNameFirstCharContext.Delete(partitionKey, user.UserId, key);
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
        foreach (var follower in followers)
        {
            var followersByChars = KeyHelper.GetKeysByFirstChars(
                follower,
                Math.Min(newCharsCount, oldCharsCount),
                Math.Abs(newCharsCount - oldCharsCount));
            foreach (var followerByChars in followersByChars)
            {
                var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(follower);
                firstCharSearchUser.FirstChars = followerByChars;
                firstCharSearchUser.PartitionKey = KeyHelper.CreateKeyByChars(user.UserId, followerByChars);
                await FirstCharsContext.Add(firstCharSearchUser);
            }
        }
    }

    private async Task HandleDowngrade(UserEntity user, int newCharsCount)
    {
        var firstCharsValues = await firstCharMapContext.Get(user.UserId, FirstCharsEntityType.FirstChars);
        var firstCharsToRemove = firstCharsValues.Where(firstCharsValue => firstCharsValue.FirstChars.Length > newCharsCount);
        await Task.WhenAll(
            firstCharsToRemove
                .Select(firstCharToRemove =>
                {
                    var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, firstCharToRemove.FirstChars);
                    return FirstCharsContext.Delete(partitionKey, firstCharToRemove.FirstChars);
                }));
    }

    private async Task UpdateFirstCharsContext(
        UserEntity user,
        UserEntity userToUpdate,
        bool increase)
    {
        var lastNameFirstChar = userToUpdate.LastName[..1];
        var firstCharEntity = await firstCharMapContext.TryGet(user.Id, lastNameFirstChar, FirstCharsEntityType.LastName);
        if (firstCharEntity == null)
        {
            if (!increase)
            {
                throw new FirstCharEntityNotFoundException(user.Id, lastNameFirstChar, FirstCharsEntityType.LastName);
            }
            else
            {
                await firstCharMapContext.Add(new FirstCharEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = user.Id,
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
                await firstCharMapContext.Delete(user.Id, lastNameFirstChar, FirstCharsEntityType.LastName);
            }
            else
            {
                await firstCharMapContext.Update(firstCharEntity);
            }
        }
    }

    private async Task<SearchUserEntity[]> GetFlatFollowers(string userId)
    {
        var firstCharsValues = await firstCharMapContext.Get(userId, FirstCharsEntityType.LastName);
        return [
            .. (
                    await Task.WhenAll(
                        firstCharsValues
                            .Select(firstCharsValue =>
                            {
                                var partitionKey = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars);
                                return lastNameFirstCharContext.Get(partitionKey, firstCharsValue.FirstChars);
                            }))
                ).SelectMany(items => items)
        ];
    }
}
