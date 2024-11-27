namespace FitnessApp.ContactsApi.Events;

public class CategoryChangedEvent
{
    required public string UserId { get; set; }
    required public byte OldCategory { get; set; }
    required public byte NewCategory { get; set; }
}
