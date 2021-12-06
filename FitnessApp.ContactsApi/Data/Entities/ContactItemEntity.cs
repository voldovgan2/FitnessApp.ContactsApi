using FitnessApp.Abstractions.Db.Entities.Collection;

namespace FitnessApp.ContactsApi.Data.Entities
{
    public class ContactItemEntity : ICollectionItemEntity
    {
        public string Id { get; set; }
    }
}
