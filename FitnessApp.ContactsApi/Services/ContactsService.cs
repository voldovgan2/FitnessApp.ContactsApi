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

namespace FitnessApp.ContactsApi.Services;

public interface IContactsService
{
    Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserModel user);
    Task FollowUser(string userId, string userToFollowId);
    Task UnFollowUser(string userId, string userToUnFollowId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
}

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
            await storage.AddFollower(userId, userToFollowId);
            var userToFollow = await storage.GetUser(userToFollowId);
            HandleCategoryChange(
                userToFollow,
                CategoryHelper.ShouldUpgradeCategory,
                CategoryHelper.GetUpgradeCategory);
        }
    }

    public async Task UnFollowUser(string userId, string userToUnFollowId)
    {
        if (await storage.IsFollower(userId, userToUnFollowId))
        {
            await storage.RemoveFollower(userId, userToUnFollowId);
            var userToUnFollow = await storage.GetUser(userToUnFollowId);
            HandleCategoryChange(
                userToUnFollow,
                CategoryHelper.ShouldDowngradeCategory,
                CategoryHelper.GetDowngradeCategory);
        }
    }

    public Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        return storage.UpdateUser(oldUser, newUser);
    }

    private void HandleCategoryChange(
        UserEntity user,
        Func<UserEntity, DateTime, bool> shouldChangeCategory,
        Func<byte, byte> getNewCategory)
    {
        if (shouldChangeCategory(user, dateTimeService.Now))
        {
            var oldCategory = user.Category;
            var newCategory = getNewCategory(user.Category);
            serviceBus.PublishEvent(CategoryChangedEvent.Topic, JsonSerializerHelper.SerializeToBytes(new CategoryChangedEvent
            {
                UserId = user.UserId,
                OldCategory = oldCategory,
                NewCategory = newCategory,
            }));
        }
    }
}
