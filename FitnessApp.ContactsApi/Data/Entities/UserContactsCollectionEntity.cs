using System.Collections.Generic;
using FitnessApp.Common.Abstractions.Db.Entities.Collection;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.ContactsApi.Data.Entities
{
    public class UserContactsCollectionEntity : ICollectionEntity
    {
        [BsonId]
        public string UserId { get; set; }
        public Dictionary<string, List<ICollectionItemEntity>> Collection { get; set; }
    }
}
