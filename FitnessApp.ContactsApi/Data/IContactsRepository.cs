using FitnessApp.Common.Abstractions.Db.Repository.Collection;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;

namespace FitnessApp.ContactsApi.Data
{
    public interface IContactsRepository
        : ICollectionRepository<UserContactsCollectionModel, ContactCollectionItemModel, CreateUserContactsCollectionModel, UpdateUserContactCollectionModel>
    { }
}
