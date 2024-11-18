using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IContactsService
{
    Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserModel user);
    Task FollowUser(string currentUserId, string userToFollowId);
    Task UnFollowUser(string currentUserId, string userToUnFollowId);
    Task UpdateUser(UserModel oldUser, UserModel newUser);
}
