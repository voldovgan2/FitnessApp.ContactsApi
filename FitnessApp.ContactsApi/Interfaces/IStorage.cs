using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IStorage
{
    Task<UserEntity> GetUser(string userId);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<SearchUserEntity>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserEntity user);
    Task UpdateUser(UserEntity user);
    Task<bool> IsFollower(string currentUserId, string userToFollowId);
    Task AddFollower(UserEntity user, string userToFollowId);
    Task RemoveFollower(UserEntity user, string userToUnFollowId);
    Task HandleCategoryChange(CategoryChangedEvent categoryChangedEvent);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
}
