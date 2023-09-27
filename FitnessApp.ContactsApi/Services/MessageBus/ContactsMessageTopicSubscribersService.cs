using System;
using System.Threading.Tasks;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.Common.ServiceBus;
using FitnessApp.ContactsApi.Models.Input;

namespace FitnessApp.ContactsApi.Services.MessageBus
{
    public class ContactsMessageTopicSubscribersService : CollectionServiceNewUserRegisteredSubscriberService<CreateUserContactsCollectionModel>
    {
        public ContactsMessageTopicSubscribersService(
            Func<CreateUserContactsCollectionModel, Task<string>> createItemMethod,
            string subscription,
            IJsonSerializer serializer
        )
            : base(createItemMethod, subscription, serializer)
        { }
    }
}