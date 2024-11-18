using System;
using FitnessApp.ContactsApi.Interfaces;

namespace FitnessApp.ContactsApi.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now { get; set; } = DateTime.UtcNow;
}
