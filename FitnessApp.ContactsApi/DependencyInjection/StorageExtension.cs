using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.ContactsApi.DependencyInjection;

public static class StorageExtension
{
    public static IServiceCollection ConfigureStorage(this IServiceCollection services)
    {
        services.AddTransient<IGlobalContainer, GlobalContainer>();
        services.AddTransient<IFollowersContainer, FollowersContainer>();
        services.AddTransient<IContactsRepository, ContactsRepository>();
        services.AddTransient<IStorage, Storage>();
        services.AddTransient<IContactsService, ContactsService>();
        services.AddTransient<IUsersCache, UsersCache>();
        services.AddTransient<IUserDbContext, UserDbContext>();
        services.AddTransient<IFollowerDbContext, FollowerDbContext>();
        services.AddTransient<IFollowingDbContext, FollowingDbContext>();
        services.AddTransient<IFollowerRequestDbContext, FollowerRequestDbContext>();
        services.AddTransient<IFirstCharSearchUserDbContext, FirstCharSearchUserDbContext>();
        services.AddTransient<IFirstCharDbContext, FirstCharDbContext>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddTransient<ICategoryChangeHandler, CategoryChangeHandler>();
        return services;
    }
}
