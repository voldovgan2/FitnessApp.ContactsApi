using System;
using System.Threading.Tasks;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using FitnessApp.ContactsApi.Services.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace ContactsApi.Data
{
    public class DataInitializer
    {
        public static async Task EnsureContactsAreCreatedAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var service = services.GetRequiredService<IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel>> ();
                var adminEmail = "admin@hotmail.com";
                var adminId = $"ApplicationUser_{adminEmail}";
                await service.CreateItemContacts(new CreateUserContactsModel { UserId = adminId });
                for (int k = 0; k < 120; k++)
                {
                    var email = $"user{k}@hotmail.com";
                    var userId = $"ApplicationUser_{email}";
                    await service.CreateItemContacts(new CreateUserContactsModel { UserId = userId });
                    if (k < 30)
                    {
                        await service.StartFollowAsync(new SendFollowModel
                        {
                            UserId = userId,
                            UserToFollowId = adminId
                        });
                        await service.AcceptFollowRequestAsync(new ProcessFollowRequestModel
                        {
                            UserId = adminId,
                            FollowerUserId = userId
                        });
                    }
                    else if (k < 60)
                    {
                        await service.StartFollowAsync(new SendFollowModel
                        {
                            UserId = adminId,
                            UserToFollowId = userId
                        });
                        await service.AcceptFollowRequestAsync(new ProcessFollowRequestModel
                        {
                            UserId = userId,
                            FollowerUserId = adminId
                        });
                    }
                    else if (k < 90)
                    {
                        await service.StartFollowAsync(new SendFollowModel
                        {
                            UserId = adminId,
                            UserToFollowId = userId
                        });
                    }
                    else
                    {
                        await service.StartFollowAsync(new SendFollowModel
                        {
                            UserId = userId,
                            UserToFollowId = adminId
                        });
                    }
                }
            }
        }
    }
}