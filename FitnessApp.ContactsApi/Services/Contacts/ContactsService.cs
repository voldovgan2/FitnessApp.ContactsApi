using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.Enums.Collection;
using FitnessApp.Common.Abstractions.Models.Collection;
using FitnessApp.Common.Serializer;
using FitnessApp.Common.ServiceBus.Nats;
using FitnessApp.Common.ServiceBus.Nats.Events;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;

namespace FitnessApp.ContactsApi.Services.Contacts;

public class ContactsService(
    IContactsRepository repository,
    IServiceBus serviceBus,
    IMapper mapper) : IContactsService
{
    public async Task<IEnumerable<ContactCollectionItemModel>> GetUserContacts(GetUserContactsModel model)
    {
        IEnumerable<ContactCollectionItemModel> result = null;
        var contactModel = await repository.GetItemByUserId(model.UserId);
        if (contactModel != null)
        {
            string collectionName = null;
            switch (model.ContactsType)
            {
                case Enums.ContactsType.Followers:
                    collectionName = "Followers";
                    break;
                case Enums.ContactsType.Followings:
                    collectionName = "Followings";
                    break;
                case Enums.ContactsType.FollowRequests:
                    collectionName = "FollowRequests";
                    break;
                case Enums.ContactsType.FollowingsRequests:
                    collectionName = "FollowingRequests";
                    break;
            }

            if (collectionName != null)
                result = mapper.Map<IEnumerable<ContactCollectionItemModel>>(contactModel.Collection[collectionName]);
        }

        return result;
    }

    public async Task<string> CreateItemContacts(CreateUserContactsCollectionModel model)
    {
        model.Collection = new Dictionary<string, IEnumerable<ICollectionItemModel>>
        {
            { "Followers", new List<ICollectionItemModel>() },
            { "Followings", new List<ICollectionItemModel>() },
            { "FollowRequests", new List<ICollectionItemModel>() },
            { "FollowingRequests", new List<ICollectionItemModel>() }
        };
        var result = await repository.CreateItem(model);
        return result;
    }

    public async Task<bool> GetIsFollower(GetFollowerStatusModel model)
    {
        var contactModel = await repository.GetItemByUserId(model.UserId);
        var collection = contactModel.Collection["Followers"];
        bool result = collection.Exists(f => f.Id == model.ContactsUserId);
        return result;
    }

    public async Task<string> StartFollow(SendFollowModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Add, model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Add, model.UserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2
            ],
            model.UserId
        );
        if (result != null)
        {
            serviceBus.PublishEvent(Topic.NEW_FOLLOW_REQUEST, JsonConvertHelper.SerializeToBytes(new NewFollowRequest
            {
                UserId = model.UserId,
                UserToFollowId = model.UserToFollowId
            }));
        }

        return result;
    }

    public async Task<string> AcceptFollowRequest(ProcessFollowRequestModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "FollowRequests", UpdateCollectionAction.Remove, model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(model.FollowerUserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserId);
        var updateModel3 = CreateUpdateModel(model.FollowerUserId, "Followings", UpdateCollectionAction.Add, model.UserId);
        var updateModel4 = CreateUpdateModel(model.UserId, "Followers", UpdateCollectionAction.Add, model.FollowerUserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2,
                updateModel3,
                updateModel4
            ],
            model.UserId
        );
        return result;
    }

    public async Task<string> RejectFollowRequest(ProcessFollowRequestModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "FollowRequests", UpdateCollectionAction.Remove, model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(model.FollowerUserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2
            ],
            model.UserId
        );
        return result;
    }

    public async Task<string> DeleteFollowRequest(SendFollowModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Remove, model.UserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2
            ],
            model.UserId
        );
        return result;
    }

    public async Task<string> DeleteFollower(ProcessFollowRequestModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "Followers", UpdateCollectionAction.Remove, model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(model.FollowerUserId, "Followings", UpdateCollectionAction.Remove, model.UserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2
            ],
            model.UserId
        );
        return result;
    }

    public async Task<string> UnfollowUser(SendFollowModel model)
    {
        var updateModel1 = CreateUpdateModel(model.UserId, "Followings", UpdateCollectionAction.Remove, model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(model.UserToFollowId, "Followers", UpdateCollectionAction.Remove, model.UserId);
        var result = await HandleFollowRequest(
            [
                updateModel1,
                updateModel2
            ],
            model.UserId
        );
        return result;
    }

    public async Task<string> DeleteItem(string userId)
    {
        string result = (await repository.DeleteItem(userId)).UserId;
        return result;
    }

    private async Task<string> HandleFollowRequest(IEnumerable<UpdateUserContactCollectionModel> items, string userId)
    {
        await repository.UpdateItems(items);
        return userId;
    }

    private UpdateUserContactCollectionModel CreateUpdateModel(string userId, string collectionName, UpdateCollectionAction action, string changeUserId)
    {
        var model = new UpdateUserContactCollectionModel
        {
            UserId = userId,
            CollectionName = collectionName,
            Action = action,
            Model = new ContactCollectionItemModel
            {
                Id = changeUserId
            }
        };
        return model;
    }
}
