﻿using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Helpers;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Services;

public class Storage(
    IUsersCache cache,
    IContactsRepository contactsRepository,
    IDateTimeService dateTimeService) : IStorage
{
    public async Task<UserEntity> GetUser(string userId)
    {
        var user = await cache.GetUser(userId);
        if (user != null)
            return user;
        return await contactsRepository.GetUser(userId);
    }

    public async Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model)
    {
        return ConvertHelper.PagedFirstCharSearchUserEntityFromPagedUserModel(await contactsRepository.GetUsers(model));
    }

    public async Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model)
    {
        return ConvertHelper.PagedFirstCharSearchUserEntityFromPagedUserModel(await contactsRepository.GetUserFollowers(userId, model));
    }

    public async Task AddUser(UserEntity user)
    {
        var saveCacheTask = cache.SaveUser(user);
        var saveRepositoryTask = contactsRepository.AddUser(user);
        await Task.WhenAll(saveCacheTask, saveRepositoryTask);
    }

    public Task<bool> IsFollower(string userId, string userToFollowId)
    {
        return contactsRepository.IsFollower(userId, userToFollowId);
    }

    public Task AddFollower(string userId, string userToFollowId)
    {
        return contactsRepository.AddFollower(userId, userToFollowId);
    }

    public Task RemoveFollower(string userId, string userToUnFollowId)
    {
        return contactsRepository.RemoveFollower(userId, userToUnFollowId);
    }

    public Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        return contactsRepository.UpdateUser(oldUser, newUser);
    }

    public async Task HandleCategoryChange(CategoryChangedEvent @event)
    {
        await contactsRepository.HandleCategoryChange(@event);
        var user = await GetUser(@event.UserId);
        user.Category = @event.NewCategory;
        user.CategoryDate = dateTimeService.Now;
        var saveCacheTask = cache.SaveUser(user);
        var saveRepositoryTask = contactsRepository.UpdateUserFolowwersInfo(user);
        await Task.WhenAll(saveCacheTask, saveRepositoryTask);
    }
}
