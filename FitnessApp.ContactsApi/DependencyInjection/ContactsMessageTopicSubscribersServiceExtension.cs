using System;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.ContactsApi.Services.MessageBus;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.ContactsApi.DependencyInjection
{
    public static class ContactsMessageTopicSubscribersServiceExtension
    {
        public static IServiceCollection AddContactsMessageTopicSubscribersService(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddTransient(
                sp =>
                {
                    return new ContactsMessageTopicSubscribersService(
                        sp.GetRequiredService<IServiceBus>(),
                        sp.GetRequiredService<IContactsService>().CreateItemContacts);
                }
            );

            return services;
        }
    }
}
