using System;
using System.Threading.Tasks;
using FitnessApp.Common.Extensions;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Helpers;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;
using FitnessApp.ContactsApi.Interfaces;

namespace FitnessApp.ContactsApi.Services;

public class ContactsService(
    IStorage storage,
    IServiceBus serviceBus,
    IDateTimeService dateTimeService) :
    IContactsService
{
    public Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model)
    {
        return storage.GetUsers(model);
    }

    public Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model)
    {
        return storage.GetUserFollowers(userId, model);
    }

    public Task AddUser(UserModel user)
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Category = 1,
            CategoryDate = dateTimeService.Now,
            Rating = 1
        };
        return storage.AddUser(userEntity);
    }

    public async Task FollowUser(string userId, string userToFollowId)
    {
        if (!await storage.IsFollower(userId, userToFollowId))
        {
            var (User1, User2) = await GetUsersPair(userId, userToFollowId);
            await AddFollower(User1, User2);
        }
    }

    public async Task UnFollowUser(string userId, string userToUnFollowId)
    {
        if (await storage.IsFollower(userToUnFollowId, userId))
        {
            var (User1, User2) = await GetUsersPair(userId, userToUnFollowId);
            await RemoveFollower(User1, User2);
        }
    }

    public Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        return storage.UpdateUser(oldUser, newUser);
    }

    private async Task<(UserEntity User1, UserEntity User2)> GetUsersPair(string id1, string id2)
    {
        var getUser1Task = storage.GetUser(id1);
        var getUser2Task = storage.GetUser(id2);
        await Task.WhenAll(getUser1Task, getUser2Task);
        return (getUser1Task.Result, getUser2Task.Result);
    }

    private async Task AddFollower(UserEntity user, UserEntity userToFollow)
    {
        userToFollow.FollowersCount++;
        await storage.AddFollower(user, userToFollow.UserId);
        await storage.UpdateUser(userToFollow);
        await HandleCategoryChange(true, userToFollow);
    }

    private async Task RemoveFollower(UserEntity user, UserEntity userToUnFollow)
    {
        userToUnFollow.FollowersCount--;
        await storage.RemoveFollower(user, userToUnFollow.UserId);
        await storage.UpdateUser(userToUnFollow);
        await HandleCategoryChange(false, userToUnFollow);
    }

    private async Task HandleCategoryChange(bool increased, UserEntity user)
    {
        Func<UserEntity, DateTime, bool> shouldChangeCategory = increased ?
            CategoryHelper.ShouldUpgradeCategory
            : CategoryHelper.ShouldDowngradeCategory;
        if (shouldChangeCategory(user, dateTimeService.Now))
        {
            var oldCategory = user.Category;
            var newCategory = increased ?
                CategoryHelper.GetUpgradeCategory(user.Category)
                : CategoryHelper.GetDowngradeCategory(user.Category);
            user.Category = newCategory;
            user.CategoryDate = dateTimeService.Now;
            await storage.UpdateUser(user);
            serviceBus.PublishEvent("categpry_changed", JsonSerializerHelper.SerializeToBytes(new CategoryChangedEvent
            {
                UserId = user.UserId,
                OldCategory = oldCategory,
                NewCategory = newCategory,
            }));
        }
    }
}
