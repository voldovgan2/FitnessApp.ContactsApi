using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Services;

public class GlobalContainer(IFirstCharSearchUserDbContext firstCharsContext) :
    ContainerBase(firstCharsContext),
    IGlobalContainer
{
    private const int _charsCount = 2;

    public Task AddUser(UserEntity userToAdd)
    {
        return AddUserToFirstCharsContext(
            userToAdd,
            (keys) => keys,
            _charsCount);
    }

    public Task UpdateUser(UserEntity userToUpdate)
    {
        return UpdateUserInFirstCharsContext(
            userToUpdate,
            (keys) => keys,
            _charsCount);
    }

    public Task RemoveUser(UserEntity userToRemove)
    {
        return RemoveUserFromFirstCharsContext(
            userToRemove,
            (keys) => keys,
            _charsCount);
    }

    public Task UpdateUser(UserEntity oldUser1, UserEntity newUser1)
    {
        return UpdateUserInFirstCharsContext(
            oldUser1,
            newUser1,
            (keys) => keys,
            _charsCount);
    }

    public Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model)
    {
        return GetUserFromFirstCharsContext(
            model,
            (keys) => keys,
            _charsCount);
    }
}
