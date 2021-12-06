namespace FitnessApp.ContactsApi.Models.Input
{
    public class SendFollowModel
    {
        public string UserId { get; set; }
        public string UserToFollowId { get; set; }
    }
}
