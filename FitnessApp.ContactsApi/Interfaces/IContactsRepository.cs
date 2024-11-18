using System;
using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IContactsRepository
{
    Task<UserEntity> GetUser(string userId);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<SearchUserEntity>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserEntity userToAdd);
    Task<FollowRequestEntity> AddFollowRequest(string thisId, string otherId);
    Task<FollowRequestEntity> DeleteFollowRequest(string thisId, string otherId);
    Task UpdateUser(UserEntity userToUpdate);
    Task<bool> IsFollower(string currentUserId, string userToFollowId);
    Task AddFollower(UserEntity whoFollows, string userId);
    Task RemoveFollower(UserEntity whoUnFollows, string userId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
    Task HandleCategoryChange(CategoryChangedEvent categoryChangedEvent);
}
