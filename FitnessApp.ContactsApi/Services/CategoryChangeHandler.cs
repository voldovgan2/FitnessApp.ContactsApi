using System.Threading.Tasks;
using FitnessApp.ContactsApi.Events;
using FitnessApp.ContactsApi.Interfaces;

namespace FitnessApp.ContactsApi.Services;

public class CategoryChangeHandler(IStorage storage) : ICategoryChangeHandler
{
    public Task Handle(CategoryChangedEvent categoryChangedEvent)
    {
        return storage.HandleCategoryChange(categoryChangedEvent);
    }
}
