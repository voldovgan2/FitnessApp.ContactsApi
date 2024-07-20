using AutoMapper;
using FitnessApp.Common.Abstractions.Db.DbContext;
using FitnessApp.Common.Abstractions.Db.Repository.Collection;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;

namespace FitnessApp.ContactsApi.Data;

public class ContactsRepository(IDbContext<UserContactsCollectionEntity> context, IMapper mapper) :
    CollectionRepository<
        UserContactsCollectionEntity,
        ContactCollectionItemEntity,
        UserContactsCollectionModel,
        ContactCollectionItemModel,
        CreateUserContactsCollectionModel,
        UpdateUserContactCollectionModel>(context, mapper),
    IContactsRepository;
