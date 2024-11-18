using System.Threading.Tasks;
using FitnessApp.ContactsApi.Events;

namespace FitnessApp.ContactsApi.Interfaces;

public interface ICategoryChangeHandler
{
    Task Handle(CategoryChangedEvent categoryChangedEvent);
}
