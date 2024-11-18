using System;
using System.Threading.Tasks;
using FitnessApp.ContactsApi.Data;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IUsersCache
{
    Task<UserEntity> GetUser(string id);
    Task SaveUser(UserEntity user);
}
