using FitnessApp.Abstractions.Db.Entities.Collection;
using FitnessApp.Abstractions.Models.Collection;
using FitnessApp.ContactsApi.Models.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessApp.ContactsApi.Services.Contacts
{
    public interface IContactsService<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>
        where Entity : ICollectionEntity
        where CollectionItemEntity : ICollectionItemEntity
        where Model : ICollectionModel
        where CollectionItemModel : ISearchableCollectionItemModel
        where CreateModel : ICreateCollectionModel
        where UpdateModel : IUpdateCollectionModel
    {
        Task<IEnumerable<CollectionItemModel>> GetUserContacts(GetUserContactsModel model);
        Task<string> CreateItemContacts(CreateModel model);
        Task<bool?> GetIsFollowerAsync(GetFollowerStatusModel model);
        Task<string> StartFollowAsync(SendFollowModel model);
        Task<string> AcceptFollowRequestAsync(ProcessFollowRequestModel model);
        Task<string> RejectFollowRequestAsync(ProcessFollowRequestModel model);
        Task<string> DeleteFollowRequestAsync(SendFollowModel model);
        Task<string> DeleteFollowerAsync(ProcessFollowRequestModel model);
        Task<string> UnfollowUserAsync(SendFollowModel model);
        Task<string> DeleteItemAsync(string userId);
    }
}
