using System;
using System.Threading.Tasks;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.Common.ServiceBus;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.ContactsApi.Models.Input;

namespace FitnessApp.ContactsApi.Services.MessageBus
{
    public class ContactsMessageTopicSubscribersService(
        IServiceBus serviceBus,
        Func<CreateUserContactsCollectionModel, Task<string>> createItemMethod,
        IJsonSerializer serializer
        ) : CollectionServiceNewUserRegisteredSubscriberService<CreateUserContactsCollectionModel>(
            serviceBus,
            createItemMethod,
            serializer);
}