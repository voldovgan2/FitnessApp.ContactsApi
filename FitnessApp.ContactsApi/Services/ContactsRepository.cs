using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public class ContactsRepository(
        IUserDbContext usersContext,
        IFollowerDbContext userFollowersContext,
        IFollowingDbContext userFollowingsContext,
        IFollowerRequestDbContext followerRequestDbContext,
        IFollowersContainer userFollowersContainer,
        IGlobalContainer globalContainer) :
    IContactsRepository
{
    public Task<UserEntity> GetUser(string userId)
    {
        return usersContext.Get(userId);
    }

    public Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model)
    {
        return globalContainer.GetUsers(model);
    }

    public async Task<PagedDataModel<SearchUserEntity>> GetUserFollowers(string userId, GetUsersModel model)
    {
        var user = await GetUser(userId);
        return await userFollowersContainer.GetUsers(user, model);
    }

    public async Task AddUser(UserEntity user)
    {
        var addUserToContextTask = usersContext.Add(user);
        var addUserToGlbalContainerTask = globalContainer.AddUser(user);
        await Task.WhenAll(addUserToContextTask, addUserToGlbalContainerTask);
    }

    public async Task UpdateUser(UserEntity user)
    {
        var updateUserInContextTask = usersContext.UpdateUser(user);
        var updateUserInGlobalContainerTask = globalContainer.UpdateUser(user);
        await Task.WhenAll(updateUserInContextTask, updateUserInGlobalContainerTask);

        var followers = await userFollowersContext.Find(user.UserId);
        var users = await Task.WhenAll(followers.Select(following => GetUser(following.FollowerId)));
        await Task.WhenAll(users.Select(u => userFollowersContainer.UpdateUser(u, user)));
    }

    public Task<FollowRequestEntity> AddFollowRequest(string thisId, string otherId)
    {
        return followerRequestDbContext.Add(thisId, otherId);
    }

    public Task<FollowRequestEntity> DeleteFollowRequest(string thisId, string otherId)
    {
        return followerRequestDbContext.Delete(thisId, otherId);
    }

    public async Task<bool> IsFollower(string currentUserId, string userToFollowId)
    {
        return await userFollowersContext
            .Find(userToFollowId, currentUserId) != null;
    }

    public async Task AddFollower(UserEntity follower, string userId)
    {
        if (!await IsFollower(follower.UserId, userId))
        {
            var user = await GetUser(userId);
            var addUserToFollowersContainerTask = userFollowersContainer.AddUser(user, follower);
            var addUserToFollowersContextTask = userFollowersContext.Add(new MyFollowerEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = follower.UserId,
                FollowerId = user.UserId
            });
            var addUserToFollowingsContextTask = userFollowingsContext.Add(new MeFollowingEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = user.UserId,
                FollowingId = follower.UserId,
            });
            await Task.WhenAll(
                addUserToFollowersContainerTask,
                addUserToFollowersContextTask,
                addUserToFollowingsContextTask);
        }
    }

    public async Task RemoveFollower(UserEntity follower, string userId)
    {
        var user = await GetUser(userId);
        var deleteFromFollowersContextTask = userFollowersContext.Delete(follower.UserId, user.UserId);
        var deleteFromFollowingsContextTask = userFollowingsContext.Delete(user.UserId, follower.UserId);
        var deleteFromFollowersContainerTask = userFollowersContainer.RemoveUser(user, follower);
        await Task.WhenAll(deleteFromFollowersContextTask, deleteFromFollowingsContextTask, deleteFromFollowersContainerTask);
    }

    public async Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        await globalContainer.UpdateUser(oldUser, newUser);
        var followings = await userFollowingsContext.Find(oldUser.UserId);
        var users = await Task.WhenAll(followings.Select(following => GetUser(following.UserId)));
        await Task.WhenAll(users.Select(user => userFollowersContainer.UpdateUser(user, oldUser, newUser)));
        await usersContext.UpdateUser(newUser);
    }

    public async Task HandleCategoryChange(CategoryChangedEvent categoryChangedEvent)
    {
        var user = await GetUser(categoryChangedEvent.UserId);
        await userFollowersContainer.HandleCategoryChange(user, categoryChangedEvent);
    }
}
