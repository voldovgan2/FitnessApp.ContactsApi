using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IContactsRepository
{
    Task<UserEntity> GetUser(string userId);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<SearchUserEntity>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserEntity user);
    Task<FollowRequestEntity> AddFollowRequest(string thisId, string otherId);
    Task<FollowRequestEntity> DeleteFollowRequest(string thisId, string otherId);
    Task UpdateUser(UserEntity user);
    Task<bool> IsFollower(string currentUserId, string userToFollowId);
    Task AddFollower(UserEntity follower, string userId);
    Task RemoveFollower(UserEntity follower, string userId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
    Task HandleCategoryChange(CategoryChangedEvent @event);
}
