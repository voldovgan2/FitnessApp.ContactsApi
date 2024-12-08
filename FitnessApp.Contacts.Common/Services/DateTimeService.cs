using FitnessApp.Contacts.Common.Interfaces;

namespace FitnessApp.Contacts.Common.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now { get; set; } = DateTime.UtcNow;
}
