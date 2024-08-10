using FitnessApp.Common.Abstractions.Db.Entities.Collection;
using FitnessApp.Common.IntegrationTests;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Enums;

namespace FitnessApp.ContactsApi.IntegrationTests;
public class MongoDbFixture : MongoDbFixtureBase<UserContactsCollectionEntity>
{
    protected override UserContactsCollectionEntity CreateEntity(string id)
    {
        var entity = base.CreateEntity(id);
        entity.Collection = Enum.GetNames(typeof(ContactsType))
            .Select(name => new KeyValuePair<string, List<ICollectionItemEntity>>(name, new List<ICollectionItemEntity>
            {
                new ContactCollectionItemEntity
                {
                    Id = ContactsIdsConstants.FollowerId,
                },
                new ContactCollectionItemEntity
                {
                    Id = ContactsIdsConstants.FollowingId,
                },
                new ContactCollectionItemEntity
                {
                    Id = ContactsIdsConstants.FollowRequestId,
                },
                new ContactCollectionItemEntity
                {
                    Id = ContactsIdsConstants.FollowingsRequestId,
                },
            }))
            .ToDictionary(key => key.Key, value => value.Value);
        return entity;
    }
}