using AutoMapper;
using FitnessApp.Common.Abstractions.Db.DbContext;
using FitnessApp.Common.Abstractions.Db.Repository.Collection;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using Microsoft.Extensions.Logging;

namespace FitnessApp.ContactsApi.Data
{
    public class ContactsRepository
        : CollectionRepository<UserContactsCollectionEntity, ContactCollectionItemEntity, UserContactsCollectionModel, ContactCollectionItemModel, CreateUserContactsCollectionModel, UpdateUserContactCollectionModel>,
        IContactsRepository
    {
        public ContactsRepository(
            IDbContext<UserContactsCollectionEntity> context,
            IMapper mapper,
            ILogger<CollectionRepository<UserContactsCollectionEntity, ContactCollectionItemEntity, UserContactsCollectionModel, ContactCollectionItemModel, CreateUserContactsCollectionModel, UpdateUserContactCollectionModel>> log
        )
            : base(context, mapper)
        { }
    }
}
