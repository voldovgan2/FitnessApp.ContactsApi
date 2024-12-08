namespace FitnessApp.Contacts.Common.Events;

public class CategoryChangedEvent
{
    public const string Topic = "category_changed";
    required public string UserId { get; set; }
    required public byte OldCategory { get; set; }
    required public byte NewCategory { get; set; }
}
