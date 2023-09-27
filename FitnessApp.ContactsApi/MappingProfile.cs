using System.Collections.Generic;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.Entities.Collection;
using FitnessApp.ContactsApi.Contracts.Input;
using FitnessApp.ContactsApi.Contracts.Output;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;

namespace FitnessApp.ContactsApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Contract 2 GenericModel
            CreateMap<CreateUserContactsContract, CreateUserContactsCollectionModel>();
            CreateMap<GetUserContactsContract, GetUserContactsModel>();
            CreateMap<GetFollowerStatusContract, GetFollowerStatusModel>();
            CreateMap<SendFollowContract, SendFollowModel>();
            CreateMap<ProcessFollowRequestContract, ProcessFollowRequestModel>();
            #endregion

            #region CollectionModel 2 CollectionEntity
            CreateMap<UserContactsCollectionModel, UserContactsCollectionEntity>();
            CreateMap<CreateUserContactsCollectionModel, UserContactsCollectionEntity>();
            #endregion

            #region CollectionItemModel 2 CollectionItemEntity
            CreateMap<ContactCollectionItemModel, ContactCollectionItemEntity>();
            #endregion

            #region CollectionItemEntity 2 CollectionItemModel
            CreateMap<ContactCollectionItemEntity, ContactCollectionItemModel>();
            #endregion

            #region CollectionEntity 2 CollectionModel
            CreateMap<UserContactsCollectionEntity, UserContactsCollectionModel>()
                .ForMember(e => e.Collection, m => m.MapFrom(o => CollectionEntitiesToCollectionModels(o.Collection)));
            #endregion

            #region CollectionItemModel 2 Contract
            CreateMap<ContactCollectionItemModel, UserContactsContract>()
                .ForMember(c => c.UserId, m => m.MapFrom(i => i.Id));
            CreateMap<string, UserContactsContract>()
                .ForMember(c => c.UserId, m => m.MapFrom(i => i));
            #endregion
        }

        private Dictionary<string, List<ContactCollectionItemModel>> CollectionEntitiesToCollectionModels(Dictionary<string, List<ICollectionItemEntity>> collections)
        {
            var result = new Dictionary<string, List<ContactCollectionItemModel>>();
            foreach (var kvp in collections)
            {
                var list = new List<ContactCollectionItemModel>();
                foreach (var item in kvp.Value)
                {
                    list.Add(new ContactCollectionItemModel
                    {
                        Id = item.Id
                    });
                }

                result.Add(kvp.Key, list);
            }

            return result;
        }
    }
}
