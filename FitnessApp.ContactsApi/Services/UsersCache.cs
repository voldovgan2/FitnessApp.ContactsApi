using System;
using System.Threading.Tasks;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Exceptions;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FitnessApp.ContactsApi.Services;

public class UsersCache(IDistributedCache distributedCache) : IUsersCache
{
    public Task<UserEntity> GetUser(string id)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        var key = CreateKey(id);
        ArgumentNullException.ThrowIfNull(key);
        UserEntity result = null;
        return Task.FromResult(result);

        // return await CacheHelper.LoadData<UserEntity>(distributedCache, CreateKey(id)) ??
        //     throw new UsersCacheException($"User with id {id} doesn't exist");
    }

    public Task SaveUser(UserEntity user)
    {
        return Task.CompletedTask;

        // return CacheHelper.SaveData(distributedCache, CreateKey(user.UserId), user);
    }

    private static string CreateKey(string userId)
    {
        return $"Users_{userId}";
    }
}
