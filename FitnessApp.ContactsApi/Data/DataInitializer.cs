using System;
using System.Threading.Tasks;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Services.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsApi.Data;

public static class DataInitializer
{
    public static async Task EnsureContactsAreCreatedAsync(IServiceProvider serviceProvider)
    {
        await Task.Delay(1000);
        /*
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            var service = services.GetRequiredService<IContactsService> ();
            for (int k = 0; k < 4; k++)
                await service.CreateItemContacts(new CreateUserContactsCollectionModel { UserId = $"user{k}" });

            var adminEmail = "admin@hotmail.com";
            var adminId = $"ApplicationUser_{adminEmail}";
            await service.CreateItemContacts(new CreateUserContactsCollectionModel { UserId = adminId });
            for (int k = 0; k < 120; k++)
            {
                var email = $"user{k}@hotmail.com";
                var userId = $"ApplicationUser_{email}";
                await service.CreateItemContacts(new CreateUserContactsCollectionModel { UserId = userId });
                if (k < 30)
                {
                    await service.StartFollow(new SendFollowModel
                    {
                        UserId = userId,
                        UserToFollowId = adminId
                    });
                    await service.AcceptFollowRequest(new ProcessFollowRequestModel
                    {
                        UserId = adminId,
                        FollowerUserId = userId
                    });
                }
                else if (k < 60)
                {
                    await service.StartFollow(new SendFollowModel
                    {
                        UserId = adminId,
                        UserToFollowId = userId
                    });
                    await service.AcceptFollowRequest(new ProcessFollowRequestModel
                    {
                        UserId = userId,
                        FollowerUserId = adminId
                    });
                }
                else if (k < 90)
                {
                    await service.StartFollow(new SendFollowModel
                    {
                        UserId = adminId,
                        UserToFollowId = userId
                    });
                }
                else
                {
                    await service.StartFollow(new SendFollowModel
                    {
                        UserId = userId,
                        UserToFollowId = adminId
                    });
                }
            }
        }
        */
    }
}