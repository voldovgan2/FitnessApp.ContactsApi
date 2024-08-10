using System.ComponentModel;

namespace FitnessApp.ContactsApi.Enums;

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