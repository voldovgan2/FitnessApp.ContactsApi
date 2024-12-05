namespace FitnessApp.Contacts.Common.Events;

public class CategoryChangedEvent
{
    required public string UserId { get; set; }
    required public byte OldCategory { get; set; }
    required public byte NewCategory { get; set; }
}
