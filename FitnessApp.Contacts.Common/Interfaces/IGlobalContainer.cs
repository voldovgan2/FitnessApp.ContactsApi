using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IGlobalContainer
{
    Task AddUser(UserEntity userToAdd);
    Task UpdateUser(UserEntity userToUpdate);
    Task RemoveUser(UserEntity userToRemove);
    Task UpdateUser(UserEntity oldUser1, UserEntity newUser1);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model);
}
