using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Interfaces;

namespace FitnessApp.ContactsCategoryHandler;

public class CategoryChangeHandler(IUserDbContext usersContext, IFollowersContainer userFollowersContainer) : ICategoryChangeHandler
{
    public async Task Handle(CategoryChangedEvent @event)
    {
        var user = await usersContext.Get(@event.UserId);
        await userFollowersContainer.HandleCategoryChange(user, @event);
    }
}
