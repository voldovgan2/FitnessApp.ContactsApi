using FitnessApp.Abstractions.Db.Configuration;
using FitnessApp.Abstractions.Db.Entities.Collection;
using FitnessApp.Abstractions.Db.Repository.Collection;
using FitnessApp.Abstractions.Models.Collection;
using FitnessApp.Serializer.JsonMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitnessApp.ContactsApi.Data
{
    public class ContactsRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>
        : CollectionRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>,
        IContactsRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>
        where Entity : ICollectionEntity
        where CollectionItemEntity : ICollectionItemEntity
        where Model : ICollectionModel
        where CollectionItemModel : ISearchableCollectionItemModel
        where CreateModel : ICreateCollectionModel
        where UpdateModel : IUpdateCollectionModel
    {
        public ContactsRepository
        (
            IOptions<MongoDbSettings> settings,
            IJsonMapper mapper,
            ILogger<CollectionRepository<Entity, CollectionItemEntity, Model, CollectionItemModel, CreateModel, UpdateModel>> log
        )
            : base(settings, mapper, log)
        {

        }
    }
}
