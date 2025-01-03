using System.ComponentModel;

namespace FitnessApp.ContactsApi.Contracts;

public class WithUserIdContract
{
    public string UserId { get; set; }
}

public class CreateUserContactsContract : WithUserIdContract;

public class GetFollowersStatusContract
{
    public string[] UserIds { get; set; }
    public string ContactsUserId { get; set; }
}

public class GetFollowerStatusContract : WithUserIdContract
{
    public string ContactsUserId { get; set; }
}

public enum ContactsType
{
    [Description("Followers")]
    Followers,
    [Description("Followings")]
    Followings,
    [Description("FollowRequests")]
    FollowRequests,
    [Description("FollowingsRequests")]
    FollowingsRequests
}

public class GetUserContactsContract : WithUserIdContract
{
    public ContactsType ContactsType { get; set; }
}

public class ProcessFollowRequestContract : WithUserIdContract
{
    public string FollowerUserId { get; set; }
}

public class SendFollowContract : WithUserIdContract
{
    public string UserToFollowId { get; set; }
}

public class FollowerStatusContract : WithUserIdContract
{
    public bool IsFollower { get; set; }
}

public class UserContactsContract : WithUserIdContract;

public class UserContactsCountContract : WithUserIdContract
{
    public int FollowersCount { get; set; }
    public int FollowingsCount { get; set; }
}
