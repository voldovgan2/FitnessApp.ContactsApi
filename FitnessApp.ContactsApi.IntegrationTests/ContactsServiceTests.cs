using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.IntegrationTests;

public class ContactsServiceTests(IContactsService contactsService)
{
    [Fact]
    public async Task SvTest()
    {
        var model = new GetUsersModel { Search = "sv", Page = 0, PageSize = 10 };
        var users = await contactsService.GetUsers(model);
        Assert.NotNull(users);
    }
}
