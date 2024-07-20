using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;

namespace FitnessApp.ContactsApi.Services.Contacts;

public interface IContactsService
{
    Task<IEnumerable<ContactCollectionItemModel>> GetUserContacts(GetUserContactsModel model);
    Task<string> CreateItemContacts(CreateUserContactsCollectionModel model);
    Task<bool> GetIsFollower(GetFollowerStatusModel model);
    Task<string> StartFollow(SendFollowModel model);
    Task<string> AcceptFollowRequest(ProcessFollowRequestModel model);
    Task<string> RejectFollowRequest(ProcessFollowRequestModel model);
    Task<string> DeleteFollowRequest(SendFollowModel model);
    Task<string> DeleteFollower(ProcessFollowRequestModel model);
    Task<string> UnfollowUser(SendFollowModel model);
    Task<string> DeleteItem(string userId);
}
