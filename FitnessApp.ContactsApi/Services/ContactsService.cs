using System;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public class ContactsService(
    IStorage storage,
    ICategoryChangeHandler categoryChangeHandler,
    IMapper mapper,
    IDateTimeService dateTimeService) :
    IContactsService
{
    public async Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model)
    {
        var data = await storage.GetUsers(model);
        return mapper.Map<PagedDataModel<UserModel>>(data);
    }

    public async Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model)
    {
        var data = await storage.GetUserFollowers(userId, model);
        return mapper.Map<PagedDataModel<UserModel>>(data);
    }

    public Task AddUser(UserModel user)
    {
        var userEntity = mapper.Map<UserEntity>(user);
        userEntity.Category = 1;
        userEntity.CategoryDate = dateTimeService.Now;
        userEntity.Rating = 1;
        userEntity.Id = Guid.NewGuid().ToString("N");
        return storage.AddUser(userEntity);
    }

    public async Task FollowUser(string currentUserId, string userToFollowId)
    {
        if (!await storage.IsFollower(currentUserId, userToFollowId))
        {
            var (User1, User2) = await GetUsersPair(currentUserId, userToFollowId);
            await AddFollower(User1, User2);
        }
    }

    public async Task UnFollowUser(string currentUserId, string userToUnFollowId)
    {
        if (await storage.IsFollower(currentUserId, userToUnFollowId))
        {
            var (User1, User2) = await GetUsersPair(currentUserId, userToUnFollowId);
            await RemoveFollower(User1, User2);
        }
    }

    public Task UpdateUser(UserModel oldUser, UserModel newUser)
    {
        return storage.UpdateUser(mapper.Map<UserEntity>(oldUser), mapper.Map<UserEntity>(newUser));
    }

    private async Task<(UserEntity User1, UserEntity User2)> GetUsersPair(string id1, string id2)
    {
        var getUser1Task = storage.GetUser(id1);
        var getUser2Task = storage.GetUser(id2);
        await Task.WhenAll(getUser1Task, getUser2Task);
        return (getUser1Task.Result, getUser2Task.Result);
    }

    private async Task AddFollower(UserEntity currentUser, UserEntity userToFollow)
    {
        userToFollow.FollowersCount++;
        await storage.AddFollower(currentUser, userToFollow.UserId);
        await storage.UpdateUser(userToFollow);
        await HandleCategoryChange(true, userToFollow);
    }

    private async Task RemoveFollower(UserEntity currentUser, UserEntity userToUnFollow)
    {
        userToUnFollow.FollowersCount--;
        await storage.RemoveFollower(currentUser, userToUnFollow.UserId);
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
            await RaiseCategoryChangedEvent(user.UserId, oldCategory, newCategory);
        }
    }

    private Task RaiseCategoryChangedEvent(string userId, byte oldCategory, byte newCategory)
    {
        return HandleCategoryChangedEvent(userId, oldCategory, newCategory);
    }

    private Task HandleCategoryChangedEvent(string userId, byte oldCategory, byte newCategory)
    {
        return categoryChangeHandler.Handle(new CategoryChangedEvent
        {
            UserId = userId,
            OldCategory = oldCategory,
            NewCategory = newCategory,
        });
    }
}
