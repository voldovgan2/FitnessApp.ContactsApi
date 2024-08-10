using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.Enums.Collection;
using FitnessApp.Common.Abstractions.Models.Collection;
using FitnessApp.Common.Serializer;
using FitnessApp.Common.ServiceBus.Nats;
using FitnessApp.Common.ServiceBus.Nats.Events;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Enums;
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
        var contactModel = await repository.GetItemByUserId(model.UserId);
        string collectionName = Enum.GetName(typeof(ContactsType), model.ContactsType);
        var result = mapper.Map<IEnumerable<ContactCollectionItemModel>>(contactModel.Collection[collectionName]);
        return result;
    }

    public async Task<string> CreateItemContacts(CreateUserContactsCollectionModel model)
    {
        model.Collection = Enum.GetNames(typeof(ContactsType))
            .Select(name => new KeyValuePair<string, IEnumerable<ICollectionItemModel>>(name, new List<ICollectionItemModel>()))
            .ToDictionary(key => key.Key, value => value.Value);
        var result = await repository.CreateItem(model);
        return result;
    }

    public async Task<bool> GetIsFollower(GetFollowerStatusModel model)
    {
        var contactModel = await repository.GetItemByUserId(model.UserId);
        var collection = contactModel.Collection[Enum.GetName(typeof(ContactsType), ContactsType.Followers)];
        bool result = collection.Exists(f => f.Id == model.ContactsUserId);
        return result;
    }

    public async Task<IEnumerable<FollowerStatusModel>> GetIsFollowers(GetFollowersStatusModel model)
    {
        var result = model.UserIds.Select(userId => new FollowerStatusModel { UserId = userId });
        foreach (var item in result)
        {
            var contactModel = await repository.GetItemByUserId(item.UserId);
            var collection = contactModel.Collection[Enum.GetName(typeof(ContactsType), ContactsType.Followers)];
            item.IsFollower = collection.Exists(f => f.Id == model.ContactsUserId);
        }

        return result;
    }

    public async Task<string> StartFollow(SendFollowModel model)
    {
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowingsRequests),
            UpdateCollectionAction.Add,
            model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(
            model.UserToFollowId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowRequests),
            UpdateCollectionAction.Add,
            model.UserId);
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
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowRequests),
            UpdateCollectionAction.Remove,
            model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(
            model.FollowerUserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowingsRequests),
            UpdateCollectionAction.Remove,
            model.UserId);
        var updateModel3 = CreateUpdateModel(
            model.FollowerUserId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followings),
            UpdateCollectionAction.Add,
            model.UserId);
        var updateModel4 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followers),
            UpdateCollectionAction.Add,
            model.FollowerUserId);
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
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowRequests),
            UpdateCollectionAction.Remove,
            model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(
            model.FollowerUserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowingsRequests),
            UpdateCollectionAction.Remove,
            model.UserId);
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
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowingsRequests),
            UpdateCollectionAction.Remove,
            model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(
            model.UserToFollowId,
            Enum.GetName(typeof(ContactsType), ContactsType.FollowRequests),
            UpdateCollectionAction.Remove,
            model.UserId);
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
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followers),
            UpdateCollectionAction.Remove,
            model.FollowerUserId);
        var updateModel2 = CreateUpdateModel(
            model.FollowerUserId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followings),
            UpdateCollectionAction.Remove,
            model.UserId);
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
        var updateModel1 = CreateUpdateModel(
            model.UserId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followings),
            UpdateCollectionAction.Remove,
            model.UserToFollowId);
        var updateModel2 = CreateUpdateModel(
            model.UserToFollowId,
            Enum.GetName(typeof(ContactsType), ContactsType.Followers),
            UpdateCollectionAction.Remove,
            model.UserId);
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
