using System.Threading.Tasks;
using FitnessApp.Contacts.Common.Data;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IUsersCache
{
    Task<UserEntity> GetUser(string id);
    Task SaveUser(UserEntity user);
}
