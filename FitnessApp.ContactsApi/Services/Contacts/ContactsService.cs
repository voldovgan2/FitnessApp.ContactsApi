using FitnessApp.Abstractions.Db.Entities.Collection;
using FitnessApp.Abstractions.Db.Enums.Collection;
using FitnessApp.Abstractions.Models.Collection;
using FitnessApp.Abstractions.Services.Collection;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.IntegrationEvents;
using FitnessApp.NatsServiceBus;
using FitnessApp.Serializer.JsonMapper;
using FitnessApp.Serializer.JsonSerializer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessApp.ContactsApi.Services.Contacts
{
    public class ContactsService<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel> :
        IContactsService<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>
        where Entity : ICollectionEntity
        where CollectionItemEntity : ICollectionItemEntity
        where Model : ICollectionModel
        where CollectionItemModel : ISearchableCollectionItemModel
        where CreateModel : ICreateCollectionModel
        where UpdateModel : IUpdateCollectionModel
    {
        private readonly IContactsRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel> _repository;
        private readonly IServiceBus _serviceBus;
        private readonly IJsonMapper _mapper;
        private readonly IJsonSerializer _serializer;

        private class InternalUpdateModel
        {
            public string UserId { get; }
            public string CollectionName { get; }
            public UpdateCollectionAction Action { get; }
            public string ChangeUserId { get; }

            public InternalUpdateModel(string userId, string collectionName, UpdateCollectionAction action, string changeUserId)
            {
                UserId = userId;
                CollectionName = collectionName;
                Action = action;
                ChangeUserId = changeUserId;
            }
        }

        public ContactsService
        (
            IContactsRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel> repository,
            IServiceBus serviceBus,
            IJsonMapper mapper,
            IJsonSerializer serializer,
            ILogger<CollectionService<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>> log
        )
        {
            _repository = repository;
            _serviceBus = serviceBus;
            _mapper = mapper;
            _serializer = serializer;
        }

        public async Task<IEnumerable<CollectionItemModel>> GetUserContacts(GetUserContactsModel model)
        {
            IEnumerable<CollectionItemModel> result = null;
            var contactModel = await _repository.GetItemByUserIdAsync(model.UserId);            
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
                if(collectionName != null)
                {
                    result = _mapper.Convert<IEnumerable<CollectionItemModel>>(contactModel.Collection[collectionName]);
                }
            }
            return result;
        }

        public async Task<string> CreateItemContacts(CreateModel model)
        {
            model.Collection = new Dictionary<string, IEnumerable<ICollectionItemModel>>
            {
                { "Followers", new List<ICollectionItemModel>() },
                { "Followings", new List<ICollectionItemModel>() },
                { "FollowRequests", new List<ICollectionItemModel>() },
                { "FollowingRequests", new List<ICollectionItemModel>() }
            };
            var result = await _repository.CreateItemAsync(model);
            return result;
        }

        public async Task<bool?> GetIsFollowerAsync(GetFollowerStatusModel model)
        {
            bool? result = null;
            var contactModel = await _repository.GetItemByUserIdAsync(model.UserId);
            var collection = contactModel.Collection["Followers"];
            result = collection.Any(f => f.Id == model.UserId);
            return result;
        }

        public async Task<string> StartFollowAsync(SendFollowModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Add, model.UserToFollowId);
            var internalUpdateModel2 = new InternalUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Add, model.UserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[] 
                {
                    internalUpdateModel1, 
                    internalUpdateModel2 
                }, 
                model.UserId
            );
            if (result != null)
            {
                _serviceBus.PublishEvent(Topic.NEW_FOLLOW_REQUEST, _serializer.SerializeToBytes(new NewFollowRequestEvent
                {
                    UserId = model.UserId,
                    UserToFollowId = model.UserToFollowId
                }));
            }
            return result;
        }

        public async Task<string> AcceptFollowRequestAsync(ProcessFollowRequestModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "FollowRequests", UpdateCollectionAction.Remove, model.FollowerUserId);
            var internalUpdateModel2 = new InternalUpdateModel(model.FollowerUserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserId);
            var internalUpdateModel3 = new InternalUpdateModel(model.FollowerUserId, "Followings", UpdateCollectionAction.Add, model.UserId);
            var internalUpdateModel4 = new InternalUpdateModel(model.UserId, "Followers", UpdateCollectionAction.Add, model.FollowerUserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[]
                {
                    internalUpdateModel1,
                    internalUpdateModel2,
                    internalUpdateModel3,
                    internalUpdateModel4
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> RejectFollowRequestAsync(ProcessFollowRequestModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "FollowRequests", UpdateCollectionAction.Remove, model.FollowerUserId);
            var internalUpdateModel2 = new InternalUpdateModel(model.FollowerUserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[]
                {
                    internalUpdateModel1,
                    internalUpdateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteFollowRequestAsync(SendFollowModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "FollowingRequests", UpdateCollectionAction.Remove, model.UserToFollowId);
            var internalUpdateModel2 = new InternalUpdateModel(model.UserToFollowId, "FollowRequests", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[]
                {
                    internalUpdateModel1,
                    internalUpdateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteFollowerAsync(ProcessFollowRequestModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "Followers", UpdateCollectionAction.Remove, model.FollowerUserId);
            var internalUpdateModel2 = new InternalUpdateModel(model.FollowerUserId, "Followings", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[]
                {
                    internalUpdateModel1,
                    internalUpdateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> UnfollowUserAsync(SendFollowModel model)
        {
            var internalUpdateModel1 = new InternalUpdateModel(model.UserId, "Followings", UpdateCollectionAction.Remove, model.UserToFollowId);
            var internalUpdateModel2 = new InternalUpdateModel(model.UserToFollowId, "Followers", UpdateCollectionAction.Remove, model.UserId);
            var result = await HandleFollowRequest
            (
                new InternalUpdateModel[]
                {
                    internalUpdateModel1,
                    internalUpdateModel2
                },
                model.UserId
            );
            return result;
        }

        public async Task<string> DeleteItemAsync(string userId)
        {
            string result = await _repository.DeleteItemAsync(userId);
            return result;
        }

        private async Task<string> HandleFollowRequest(IEnumerable<InternalUpdateModel> items, string userId)
        {
            var models = items.Select(item =>  
            {
                UpdateModel model = _mapper.Convert<UpdateModel>(item);
                CollectionItemModel modelModel = _mapper.Convert<CollectionItemModel>(new { });
                modelModel.Id = item.ChangeUserId;
                model.Model = modelModel;
                return model;
            });
            var updateResult = await _repository.UpdateItemsAsync(models);
            if (updateResult != null)
            {
                throw new Exception($"Failed to update items: {updateResult}");
            }
            return updateResult == null ? 
                userId 
                : null;
        }
    }
}
