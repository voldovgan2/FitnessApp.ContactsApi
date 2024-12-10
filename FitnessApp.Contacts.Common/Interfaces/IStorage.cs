using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IStorage
{
    Task<UserEntity> GetUser(string userId);
    Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserEntity user);
    Task<bool> IsFollower(string userId, string userToFollowId);
    Task AddFollower(string userId, string userToFollowId);
    Task RemoveFollower(string userId, string userToUnFollowId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
    Task HandleCategoryChange(CategoryChangedEvent @event);
}
