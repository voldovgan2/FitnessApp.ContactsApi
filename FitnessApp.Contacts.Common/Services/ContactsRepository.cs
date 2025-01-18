using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;
using MongoDB.Driver;

namespace FitnessApp.Contacts.Common.Services;

public class ContactsRepository(
    IMongoClient mongoClient,
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

    public Task AddUser(UserEntity user)
    {
        return ExecuteTransaction(() =>
        {
            var addUserToContextTask = usersContext.Add(user);
            var addUserToGlbalContainerTask = globalContainer.AddUser(user);
            return Task.WhenAll(addUserToContextTask, addUserToGlbalContainerTask);
        });
    }

    public Task UpdateUserFolowwersInfo(UserEntity user)
    {
        return ExecuteTransaction(() => UpdateUser(user));
    }

    public Task<FollowRequestEntity> AddFollowRequest(string thisId, string otherId)
    {
        return followerRequestDbContext.Add(thisId, otherId);
    }

    public Task<FollowRequestEntity> DeleteFollowRequest(string thisId, string otherId)
    {
        return followerRequestDbContext.Delete(thisId, otherId);
    }

    public async Task<bool> IsFollower(string userId, string userToFollowId)
    {
        return await userFollowersContext.Find(userId, userToFollowId) != null;
    }

    public Task AddFollower(string followerId, string userId)
    {
        return ExecuteTransaction(async () =>
        {
            var user = await GetUser(userId);
            var follower = await GetUser(followerId);
            user.FollowersCount += 1;
            var addUserToFollowersContainerTask = userFollowersContainer.AddUser(user, follower);
            var addUserToFollowersContextTask = userFollowersContext.Add(new MyFollowerEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = followerId,
                FollowerId = user.UserId
            });
            var addUserToFollowingsContextTask = userFollowingsContext.Add(new MeFollowingEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = user.UserId,
                FollowingId = followerId,
            });
            var updateUserTask = UpdateUser(user);
            await Task.WhenAll(
                addUserToFollowersContainerTask,
                addUserToFollowersContextTask,
                addUserToFollowingsContextTask,
                updateUserTask);
        });
    }

    public Task RemoveFollower(string followerId, string userId)
    {
        return ExecuteTransaction(async () =>
        {
            var user = await GetUser(userId);
            var follower = await GetUser(followerId);
            user.FollowersCount -= 1;
            var deleteFromFollowersContextTask = userFollowersContext.Delete(followerId, user.UserId);
            var deleteFromFollowingsContextTask = userFollowingsContext.Delete(user.UserId, followerId);
            var deleteFromFollowersContainerTask = userFollowersContainer.RemoveUser(user, follower);
            var updateUserTask = UpdateUser(user);
            await Task.WhenAll(
                deleteFromFollowersContextTask,
                deleteFromFollowingsContextTask,
                deleteFromFollowersContainerTask,
                updateUserTask);
        });
    }

    public Task UpdateUser(UserEntity oldUser, UserEntity newUser)
    {
        return ExecuteTransaction(async () =>
        {
            await globalContainer.UpdateUser(oldUser, newUser);
            var followings = await userFollowingsContext.Find(oldUser.UserId);
            var users = await Task.WhenAll(followings.Select(following => GetUser(following.UserId)));
            await Task.WhenAll(users.Select(user => userFollowersContainer.UpdateUser(user, oldUser, newUser)));
            await usersContext.UpdateUser(newUser);
        });
    }

    public Task HandleCategoryChange(CategoryChangedEvent @event)
    {
        return ExecuteTransaction(() =>
        {
            return userFollowersContainer.HandleCategoryChange(@event);
        });
    }

    private async Task UpdateUser(UserEntity user)
    {
        var updateUserInContextTask = usersContext.UpdateUser(user);
        var updateUserInGlobalContainerTask = globalContainer.UpdateUser(user);
        await Task.WhenAll(updateUserInContextTask, updateUserInGlobalContainerTask);

        var followers = await userFollowersContext.Find(user.UserId);
        var users = await Task.WhenAll(followers.Select(following => GetUser(following.FollowerId)));
        await Task.WhenAll(users.Select(u => userFollowersContainer.UpdateUser(u, user)));
    }

    private async Task ExecuteTransaction(Func<Task> func)
    {
        if (nameof(ContactsRepository).Length == "ContactsRepository".Length)
        {
            await func();
            return;
        }

        using var session = await mongoClient.StartSessionAsync();
        try
        {
            session.StartTransaction();
            await func();
            await session.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync();
        }
    }
}
