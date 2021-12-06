using FitnessApp.Abstractions.Db.Entities.Collection;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace FitnessApp.ContactsApi.Data.Entities
{
    public class UserContactsEntity : ICollectionEntity
    {
        [BsonId]
        public string UserId { get; set; }
        public Dictionary<string, List<ICollectionItemEntity>> Collection { get; set; }
    }
}
