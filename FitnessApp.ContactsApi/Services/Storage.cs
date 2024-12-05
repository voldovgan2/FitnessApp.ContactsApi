﻿using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Helpers;
using FitnessApp.Contacts.Common.Models;
using FitnessApp.ContactsApi.Interfaces;

namespace FitnessApp.ContactsApi.Services;

public class Storage(IUsersCache cache, IContactsRepository contactsRepository) : IStorage
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

    public async Task UpdateUser(UserEntity user)
    {
        var saveCacheTask = cache.SaveUser(user);
        var saveRepositoryTask = contactsRepository.UpdateUser(user);
        await Task.WhenAll(saveCacheTask, saveRepositoryTask);
    }

    public Task<bool> IsFollower(string currentUserId, string userToFollowId)
    {
        return contactsRepository.IsFollower(currentUserId, userToFollowId);
    }

    public Task AddFollower(UserEntity user, string userToFollowId)
    {
        return contactsRepository.AddFollower(user, userToFollowId);
    }

    public Task RemoveFollower(UserEntity user, string userToUnFollowId)
    {
        return contactsRepository.RemoveFollower(user, userToUnFollowId);
    }

    public Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        return contactsRepository.UpdateUser(oldUser, newUser);
    }
}
