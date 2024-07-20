namespace FitnessApp.ContactsApi.Contracts.Input;

public class SendFollowContract
{
    public string UserId { get; set; }
    public string UserToFollowId { get; set; }
}