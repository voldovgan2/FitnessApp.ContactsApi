using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.Enums.Collection;
using FitnessApp.Common.Abstractions.Models.Collection;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using FitnessApp.ServiceBus.AzureServiceBus.Producer;

namespace FitnessApp.ContactsApi.Services.Contacts
{
    public class ContactsService : IContactsService
    {
        private readonly IContactsRepository _repository;
#pragma warning disable S4487 // Unread "private" fields should be removed
        private readonly IMessageProducer _messageProducer;
#pragma warning restore S4487 // Unread "private" fields should be removed
        private readonly IMapper _mapper;
#pragma warning disable S4487 // Unread "private" fields should be removed
        private readonly IJsonSerializer _serializer;
#pragma warning restore S4487 // Unread "private" fields should be removed

        public ContactsService(
            IContactsRepository repository,
            IMessageProducer messageProducer,
            IMapper mapper,
            IJsonSerializer serializer)
        {
            _repository = repository;
            _messageProducer = messageProducer;
            _mapper = mapper;
            _serializer = serializer;
        }

        public async Task<IEnumerable<ContactCollectionItemModel>> GetUserContacts(GetUserContactsModel model)
        {
            IEnumerable<ContactCollectionItemModel> result = null;
            var contactModel = await _repository.GetItemByUserId(model.UserId);
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
                    result = _mapper.Map<IEnumerable<ContactCollectionItemModel>>(contactModel.Collection[collectionName]);
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
            var result = await _repository.CreateItem(model);
            return result;
        }

        public async Task<bool?> GetIsFollower(GetFollowerStatusModel model)
        {
            bool? result = null;
            var contactModel = await _repository.GetItemByUserId(model.UserId);
            var collection = contactModel.Collection["Followers"];
            result = collection.Exists(f => f.Id == model.ContactsUserId);
            return result;
        }

        public async Task<string> StartFollow(SendFollowModel model)
        {
            var updateModel1 = CreateUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Add, model.UserToFollowId);
            var updateModel2 = CreateUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Add, model.UserId);
            var result = await HandleFollowRequest(
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2
                },
                model.UserId
            );
            if (result != null)
            {
                /*
                _messageProducer.SendMessage(Topic.NEW_FOLLOW_REQUEST, _serializer.SerializeToBytes(new NewFollowRequestEvent
                {
                    UserId = model.UserId,
                    UserToFollowId = model.UserToFollowId
                }));
                */
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
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2,
                    updateModel3,
                    updateModel4
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> RejectFollowRequest(ProcessFollowRequestModel model)
        {
            var updateModel1 = CreateUpdateModel(model.UserId, "FollowRequests", UpdateCollectionAction.Remove, model.FollowerUserId);
            var updateModel2 = CreateUpdateModel(model.FollowerUserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest(
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteFollowRequest(SendFollowModel model)
        {
            var updateModel1 = CreateUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserToFollowId);
            var updateModel2 = CreateUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest(
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteFollower(ProcessFollowRequestModel model)
        {
            var updateModel1 = CreateUpdateModel(model.UserId, "Followers", UpdateCollectionAction.Remove, model.FollowerUserId);
            var updateModel2 = CreateUpdateModel(model.FollowerUserId, "Followings", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest(
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> UnfollowUser(SendFollowModel model)
        {
            var updateModel1 = CreateUpdateModel(model.UserId, "Followings", UpdateCollectionAction.Remove, model.UserToFollowId);
            var updateModel2 = CreateUpdateModel(model.UserToFollowId, "Followers", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest(
                new UpdateUserContactCollectionModel[]
                {
                    updateModel1,
                    updateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteItem(string userId)
        {
            string result = (await _repository.DeleteItem(userId)).UserId;
            return result;
        }

        private async Task<string> HandleFollowRequest(IEnumerable<UpdateUserContactCollectionModel> items, string userId)
        {
            await _repository.UpdateItems(items);
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
}
