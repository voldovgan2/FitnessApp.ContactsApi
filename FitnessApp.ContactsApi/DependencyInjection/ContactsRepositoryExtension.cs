using System;
using AutoMapper;
using FitnessApp.Common.Abstractions.Db.DbContext;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.ContactsApi.DependencyInjection;

public static class ContactsRepositoryExtension
{
    public static IServiceCollection ConfigureContactsRepository(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IDbContext<UserContactsCollectionEntity>, DbContext<UserContactsCollectionEntity>>();
        services.AddTransient<IContactsRepository, ContactsRepository>(
            sp =>
            {
                return new ContactsRepository(sp.GetRequiredService<IDbContext<UserContactsCollectionEntity>>(), sp.GetRequiredService<IMapper>());
            }
        );

        return services;
    }
}
