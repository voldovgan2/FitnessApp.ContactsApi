using System;
using FitnessApp.Common.Serializer.JsonSerializer;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.ContactsApi.Services.MessageBus;
using FitnessApp.ServiceBus.AzureServiceBus.TopicSubscribers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.ContactsApi.DependencyInjection
{
    public static class ContactsMessageTopicSubscribersServiceExtension
    {
        public static IServiceCollection AddContactsMessageTopicSubscribersService(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<ITopicSubscribers, ContactsMessageTopicSubscribersService>(
                sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var subscription = configuration.GetValue<string>("ServiceBusSubscriptionName");
                    return new ContactsMessageTopicSubscribersService(
                        sp.GetRequiredService<IContactsService>().CreateItemContacts,
                        subscription,
                        sp.GetRequiredService<IJsonSerializer>()
                    );
                }
            );

            return services;
        }
    }
}
