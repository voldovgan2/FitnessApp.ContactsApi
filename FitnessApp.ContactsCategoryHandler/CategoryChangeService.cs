using FitnessApp.Common.Serializer;
using FitnessApp.Common.ServiceBus.Nats.Services;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Interfaces;
using NATS.Client;

namespace FitnessApp.ContactsCategoryHandler;

public interface ICategoryChangeHandler
{
    Task Handle(CategoryChangedEvent @event);
}

public class CategoryChangeHandler(IUserDbContext usersContext, IFollowersContainer userFollowersContainer) : ICategoryChangeHandler
{
    public Task Handle(CategoryChangedEvent @event)
    {
        return usersContext
            .Get(@event.UserId).ContinueWith(getUserTask => userFollowersContainer.HandleCategoryChange(getUserTask.Result.UserId, @event));
    }
}

public class CategoryChangeService(IServiceBus serviceBus, ICategoryChangeHandler categoryChangeHandler) : IHostedService
{
    private IAsyncSubscription _eventSubscription;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventSubscription = serviceBus.SubscribeEvent(CategoryChangedEvent.Topic, async (sender, args) =>
        {
            await categoryChangeHandler.Handle(JsonConvertHelper.DeserializeFromBytes<CategoryChangedEvent>(args.Message.Data));
        });
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventSubscription.Unsubscribe();
        return Task.CompletedTask;
    }
}
