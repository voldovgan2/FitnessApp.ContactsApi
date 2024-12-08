using FitnessApp.Contacts.Common.Data;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IUsersCache
{
    Task<UserEntity> GetUser(string id);
    Task SaveUser(UserEntity user);
}
