using System;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.DbContext;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessApp.ContactsApi.DependencyInjection
{
    public static class ContactsRepositoryExtension
    {
        public static IServiceCollection AddContactsRepository(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IContactsRepository, ContactsRepository>(
                sp =>
                {
                    return new ContactsRepository(
                        sp.GetRequiredService<IDbContext<UserContactsCollectionEntity>>(),
                        sp.GetRequiredService<IMapper>(),
                        sp.GetRequiredService<ILogger<ContactsRepository>>()
                    );
                }
            );

            return services;
        }
    }
}
