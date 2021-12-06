namespace FitnessApp.ContactsApi.Contracts.Input
{
    public class ProcessFollowRequestContract
    {
        public string UserId { get; set; }
        public string FollowerUserId { get; set; }
    }
}
