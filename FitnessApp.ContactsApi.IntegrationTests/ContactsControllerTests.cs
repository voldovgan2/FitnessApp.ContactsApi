using FitnessApp.Common.Abstractions.Db.Entities.Collection;
using FitnessApp.ContactsApi.Data.Entities;
using MongoDB.Bson.Serialization;

namespace FitnessApp.ContactsApi.IntegrationTests;
public class ContactsControllerTests : IClassFixture<MongoDbFixture>
{
    private readonly HttpClient _httpClient;

    public ContactsControllerTests(MongoDbFixture fixture)
    {
        BsonClassMap.TryRegisterClassMap<UserContactsCollectionEntity>(cm =>
        {
            cm.MapMember(c => c.UserId);
            cm.MapMember(c => c.Collection);
        });
        BsonClassMap.TryRegisterClassMap<ContactCollectionItemEntity>(cm =>
        {
            cm.MapMember(c => c.Id);
        });
        var appFactory = new TestWebApplicationFactory(
            fixture,
            "FitnessContacts",
            "Contacts",
            ContactsIdsConstants.IdsToSeed);
        _httpClient = appFactory.CreateHttpClient();
    }

    [Fact]
    public async Task GetUserContacts_ReturnsOk()
    {
        // Act
        var response = await _httpClient.GetAsync($"api/Contacts/GetUserContacts?UserId={ContactsIdsConstants.FollowerId}&ContactsType=0");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetIsFollower_ReturnsOk()
    {
        // Act
        var response = await _httpClient.GetAsync($"api/Contacts/GetIsFollower?UserId={ContactsIdsConstants.FollowerId}&ContactsUserId={ContactsIdsConstants.FollowingId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
