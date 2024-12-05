using FitnessApp.Contacts.Common.Events;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface ICategoryChangeHandler
{
    Task Handle(CategoryChangedEvent @event);
}
