using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.IntegrationEvents;
using FitnessApp.NatsServiceBus;
using FitnessApp.Serializer.JsonSerializer;

namespace FitnessApp.ContactsApi.Services.MessageBus
{
    public class ContactsMessageBusService : MessageBusService
    {
        private readonly IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel> _service; 
        
        public ContactsMessageBusService
        (
            IServiceBus serviceBus,
            IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel> service,
            IJsonSerializer serializer
        )
            : base(serviceBus, serializer)
        {
            _service = service;
        }

        protected override void HandleNewUserRegisteredEvent(NewUserRegisteredEvent integrationEvent)
        {
            _service.CreateItemContacts(new CreateUserContactsModel { UserId = integrationEvent.UserId });
        }
    }
}