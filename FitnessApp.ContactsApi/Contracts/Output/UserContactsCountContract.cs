namespace FitnessApp.ContactsApi.Contracts.Output
{
    public class UserContactsCountContract
    {
        public string UserId { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingsCount { get; set; }
    }
}
