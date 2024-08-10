namespace FitnessApp.ContactsApi.IntegrationTests;
public class ContactsIdsConstants
{
    public const string FollowerId = nameof(FollowerId);
    public const string FollowingId = nameof(FollowingId);
    public const string FollowRequestId = nameof(FollowRequestId);
    public const string FollowingsRequestId = nameof(FollowingsRequestId);

    public static string[] IdsToSeed = new string[]
    {
        FollowerId,
        FollowingId,
        FollowRequestId,
        FollowingsRequestId,
    };
}
