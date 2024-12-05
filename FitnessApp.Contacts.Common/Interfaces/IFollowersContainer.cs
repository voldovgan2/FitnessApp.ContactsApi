using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IFollowersContainer
{
    Task AddUser(UserEntity user, UserEntity whoFollows);
    Task UpdateUser(UserEntity user, UserEntity userToUpdate);
    Task RemoveUser(UserEntity user, UserEntity whoUnFollows);
    Task UpdateUser(UserEntity user, UserEntity oldUser, UserEntity newUser);
    Task HandleCategoryChange(UserEntity user, CategoryChangedEvent categoryChangedEvent);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(UserEntity user, GetUsersModel model);
}
