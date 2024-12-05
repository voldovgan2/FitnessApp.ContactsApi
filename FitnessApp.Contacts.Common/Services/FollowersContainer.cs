using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Exceptions;
using FitnessApp.Contacts.Common.Helpers;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Services;

public class FollowersContainer(
        IFirstCharSearchUserDbContext lastNameFirstCharContext,
        IFirstCharSearchUserDbContext firstCharsContext,
        IFirstCharDbContext firstCharMapContext) :
    ContainerBase(firstCharsContext),
    IFollowersContainer
{
    private const string _suffix = "FirstChar";

    /// <summary>
    /// Add user to container.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="whoFollows">User that follows.</param>
    /// <returns>Task.</returns>
    public async Task AddUser(UserEntity user, UserEntity whoFollows)
    {
        var addUserToFirstCharsContextTask = AddUserToFirstCharsContext(
            whoFollows,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var addUserToLastNameFirstCharCollectionTask = AddUserToLastNameFirstCharCollection(user, whoFollows);
        var updateFirstCharsMapContextTask = UpdateFirstCharsContext(
            user,
            whoFollows,
            true);
        await Task.WhenAll(
            addUserToFirstCharsContextTask,
            addUserToLastNameFirstCharCollectionTask,
            updateFirstCharsMapContextTask);
    }

    /// <summary>
    /// Update user in container(Category or followers count changed).
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="userToUpdate">User to update.</param>
    /// <returns>Task.</returns>
    public async Task UpdateUser(UserEntity user, UserEntity userToUpdate)
    {
        var updateUserInFirstCharsContextTask = UpdateUserInFirstCharsContext(
            user,
            (keys) => KeyHelper.CreateKeyByChars(userToUpdate.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(userToUpdate.Category));
        var updateUserInLastNameFirstCharCollectionTask = UpdateUserInLastNameFirstCharCollection(user, userToUpdate);
        await Task.WhenAll(
            updateUserInFirstCharsContextTask,
            updateUserInLastNameFirstCharCollectionTask);
    }

    /// <summary>
    /// Remove user from container if unfollow.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="whoUnFollows">Who unfollows.</param>
    /// <returns>Task.</returns>
    public async Task RemoveUser(UserEntity user, UserEntity whoUnFollows)
    {
        var removeUserFromFirstCharsContextTask = RemoveUserFromFirstCharsContext(
            whoUnFollows,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var deleteUserfromLastNameFirstCharCollectionTask = DeleteUserfromLastNameFirstCharCollection(user, whoUnFollows);
        var updateFirstCharsToMapContextTask = UpdateFirstCharsContext(
            user,
            whoUnFollows,
            false);
        await Task.WhenAll(
            removeUserFromFirstCharsContextTask,
            deleteUserfromLastNameFirstCharCollectionTask,
            updateFirstCharsToMapContextTask);
    }

    /// <summary>
    /// Updates user first and/or last name. As result, we need to update first chars.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="oldUser">Old user data.</param>
    /// <param name="newUser">New user data.</param>
    /// <returns>Task.</returns>
    public async Task UpdateUser(UserEntity user, UserEntity oldUser, UserEntity newUser)
    {
        var updateUserInFirstCharsContextTask = UpdateUserInFirstCharsContext(
            oldUser,
            newUser,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
        var updateUserByFirstCharTask = UpdateUserByFirstChar(user, oldUser, newUser);
        await Task.WhenAll(updateUserInFirstCharsContextTask, updateUserByFirstCharTask);
    }

    /// <summary>
    /// If user category changed, increase or decrease users with corresponding partition keys.
    /// </summary>
    /// <param name="user">User, retrieved from Db by UserId of event.</param>
    /// <param name="categoryChangedEvent">Event.</param>
    /// <returns>Task.</returns>
    /// <exception cref="FollowersContainerException">If categories delta abs is not 1 throws exception.</exception>
    public async Task HandleCategoryChange(UserEntity user, CategoryChangedEvent categoryChangedEvent)
    {
        if (Math.Abs(categoryChangedEvent.OldCategory - categoryChangedEvent.NewCategory) != 1)
            throw new FollowersContainerException($"New category({categoryChangedEvent.NewCategory}) doesn't match to expected by old category({categoryChangedEvent.OldCategory})");
        var oldCharsCount = CategoryHelper.GetCategoryCharsCount(categoryChangedEvent.OldCategory);
        var newCharsCount = CategoryHelper.GetCategoryCharsCount(categoryChangedEvent.NewCategory);
        var isUpgrating = categoryChangedEvent.OldCategory < categoryChangedEvent.NewCategory;
        if (isUpgrating)
            await HandleUpgrade(user, oldCharsCount, newCharsCount);
        else
            await HandleDowngrade(user, newCharsCount);
    }

    /// <summary>
    /// Gets paged users by paged model.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="model">Model with params.</param>
    /// <returns cref = "PagedDataModel{SearchUserEntity}">Paged model of users.</returns>
    public Task<PagedDataModel<SearchUserEntity>> GetUsers(UserEntity user, GetUsersModel model)
    {
        return GetUserFromFirstCharsContext(
            model,
            (keys) => KeyHelper.CreateKeyByChars(user.UserId, keys),
            CategoryHelper.GetCategoryCharsCount(user.Category));
    }

    /// <summary>
    /// Add user to FirstCharSearchUserDbContext with userId, first char of last name, FirstChar suffix.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="userToAdd">User to add.</param>
    /// <returns>Task.</returns>
    private async Task AddUserToLastNameFirstCharCollection(UserEntity user, UserEntity userToAdd)
    {
        var lastNameNormalized = KeyHelper.CreateKeyByChars(userToAdd.LastName[..1]);
        var firstCharSearchUser = ConvertHelper.FirstCharSearchUserEntityFromUserEntity(
            userToAdd,
            lastNameNormalized,
            KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix));
        await lastNameFirstCharContext.Add(firstCharSearchUser);
    }

    /// <summary>
    /// Updates user general info(for now just artificial rating) in search from FirstCharSearchUserDbContext with userId, first char of last name, FirstChar suffix.
    /// Not affects partition key and chars count.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="userToUpdate">Who to update.</param>
    /// <returns>Task.</returns>
    private async Task UpdateUserInLastNameFirstCharCollection(UserEntity user, UserEntity userToUpdate)
    {
        var lastNameNormalized = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, lastNameNormalized, _suffix);
        var firstCharSearchUser = await lastNameFirstCharContext.Get(new PartitionKeyAndIdAndFirstCharFilter(partitionKey, userToUpdate.UserId, lastNameNormalized));
        ConvertHelper.UpdateFirstCharSearchUserEntityByUserEntity(firstCharSearchUser, userToUpdate);
        await lastNameFirstCharContext.Update(firstCharSearchUser);
    }

    /// <summary>
    /// Remove user from FirstCharSearchUserDbContext with userId, first char of last name, FirstChar suffix.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="userToRemove">User to remove.</param>
    /// <returns>Task.</returns>
    private async Task DeleteUserfromLastNameFirstCharCollection(UserEntity user, UserEntity userToRemove)
    {
        var key = KeyHelper.CreateKeyByChars(userToRemove.LastName[..1]);
        var partitionKey = KeyHelper.CreateKeyByChars(user.UserId, key, _suffix);
        await lastNameFirstCharContext.Delete(new PartitionKeyAndIdAndFirstCharFilter(partitionKey, userToRemove.UserId, key));
    }

    /// <summary>
    /// If first char of last name changed:
    /// Delete user from FirstCharSearchUserDbContext with userId, first char of last name, FirstChar suffix.
    /// Add user with new first char of last name, FirstChar suffix.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="oldUser">Previous user info(FirstName, LastName).</param>
    /// <param name="newUser">New user info(FirstName, LastName).</param>
    /// <returns>Task.</returns>
    private async Task UpdateUserByFirstChar(UserEntity user, UserEntity oldUser, UserEntity newUser)
    {
        if (oldUser.LastName.First() != newUser.LastName.First())
        {
            var deleteUserfromLastNameFirstCharCollectionTask = DeleteUserfromLastNameFirstCharCollection(user, oldUser);
            var addUserToLastNameFirstCharCollectionTask = AddUserToLastNameFirstCharCollection(user, newUser);
            var deleteFirstCharsContextTask = UpdateFirstCharsContext(user, oldUser, false);
            var addFirstCharsContextTask = UpdateFirstCharsContext(user, newUser, true);
            await Task.WhenAll(
                deleteUserfromLastNameFirstCharCollectionTask,
                addUserToLastNameFirstCharCollectionTask,
                deleteFirstCharsContextTask,
                addFirstCharsContextTask);
        }
    }

    /// <summary>
    /// Handle upgrade - add new reccords for users with more chars in partition key.
    /// Also update MapContext - changes first chars.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="oldCharsCount">Previous category chars count.</param>
    /// <param name="newCharsCount">New category chars count.</param>
    /// <returns>Task.</returns>
    private async Task HandleUpgrade(
        UserEntity user,
        int oldCharsCount,
        int newCharsCount)
    {
        var followers = await GetFlatFollowers(user.UserId);
        await UpgradeFirstCharsContext(user, followers, oldCharsCount, newCharsCount);
        await UpgradeFirstCharsMapContext(user, followers, oldCharsCount, newCharsCount);
    }

    /// <summary>
    /// Add new reccords for users with more chars in partition key.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="followers">Array of followers to construct and add new partition keys.</param>
    /// <param name="oldCharsCount">Previous category chars count.</param>
    /// <param name="newCharsCount">New category chars count.</param>
    /// <returns>Task.</returns>
    private async Task UpgradeFirstCharsContext(
        UserEntity user,
        FirstCharSearchUserEntity[] followers,
        int oldCharsCount,
        int newCharsCount)
    {
        var users = followers.
            Select(follower =>
            {
                var followersByChars = KeyHelper.GetKeysByFirstChars(
                    follower,
                    oldCharsCount,
                    newCharsCount);
                return followersByChars.Select(followerByChars =>
                {
                    return ConvertHelper.FirstCharSearchUserEntityFromFirstCharSearchUserEntity(
                        follower,
                        followerByChars,
                        KeyHelper.CreateKeyByChars(user.UserId, followerByChars));
                });
            }).SelectMany(users => users);
        await FirstCharsContext.Add([.. users]);
    }

    /// <summary>
    /// Update FirstCharContext - after increase chars count, we add more keys to user first char keys in map context.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="followers">Array of followers to construct and add new first char keys.</param>
    /// <param name="oldCharsCount">Previous category chars count.</param>
    /// <param name="newCharsCount">New category chars count.</param>
    /// <returns>Task.</returns>
    private async Task UpgradeFirstCharsMapContext(
        UserEntity user,
        SearchUserEntity[] followers,
        int oldCharsCount,
        int newCharsCount)
    {
        var followerByChars = followers.
            Select(follower =>
            {
                return KeyHelper.GetKeysByFirstChars(
                    follower,
                    oldCharsCount,
                    newCharsCount);
            }).SelectMany(firstCharKeys => firstCharKeys);
        await Task.WhenAll(followerByChars.Select(followerByChar => UpdateFirstCharsContext(
            user,
            FirstCharsEntityType.FirstChars,
            followerByChar,
            true)));
    }

    /// <summary>
    /// Add informative comment after final implementation.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="newCharsCount">New chars count.</param>
    /// <returns>Task.</returns>
    private async Task HandleDowngrade(
        UserEntity user,
        int newCharsCount)
    {
        var @params = await CreatePartitionKeyAndFirstCharParamsFromFirstCharsFirstCharsValue(user.UserId);
        await FirstCharsContext.Delete([.. @params.Where(param => param.FirstChars.Length > newCharsCount)]);
    }

    /// <summary>
    /// Add informative comment after final implementation.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="userToUpdate">User to update.</param>
    /// <param name="increase">Increase or decrease - might be changed to int +(-)1.</param>
    /// <returns>Task.</returns>
    private async Task UpdateFirstCharsContext(
        UserEntity user,
        UserEntity userToUpdate,
        bool increase)
    {
        var updateFirstCharsContextTasks = new List<Task>();
        var lastNameFirstChar = KeyHelper.CreateKeyByChars(userToUpdate.LastName[..1]);
        updateFirstCharsContextTasks.Add(UpdateFirstCharsContext(user, FirstCharsEntityType.LastName, lastNameFirstChar, increase));
        var charsCount = CategoryHelper.GetCategoryCharsCount(user.Category);
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToUpdate, 0, charsCount);
        updateFirstCharsContextTasks.AddRange(firstCharsKeys.Select(firstCharsKey => UpdateFirstCharsContext(user, FirstCharsEntityType.FirstChars, firstCharsKey, increase)));
        foreach (var task in updateFirstCharsContextTasks)
        {
            await task;
        }
    }

    /// <summary>
    /// Update context map if we add or remove user to/from context. We need it for both FirstChar and LastName.
    /// </summary>
    /// <param name="user">In which user context to execute method, used to build partition key and chars count.</param>
    /// <param name="firstCharsEntityType">FirstChar or LastName.</param>
    /// <param name="lastNameFirstChar">First char value.</param>
    /// <param name="increase">Increase or decrease followers count.</param>
    /// <returns>Task.</returns>
    /// <exception cref="FirstCharEntityNotFoundException">If not found.</exception>
    private async Task UpdateFirstCharsContext(
        UserEntity user,
        FirstCharsEntityType firstCharsEntityType,
        string lastNameFirstChar,
        bool increase)
    {
        var firstCharEntity = await firstCharMapContext.TryGet(user.UserId, lastNameFirstChar, firstCharsEntityType);
        if (firstCharEntity == null)
        {
            if (!increase)
            {
                throw new FirstCharEntityNotFoundException(user.UserId, lastNameFirstChar, firstCharsEntityType);
            }
            else
            {
                await firstCharMapContext.Add(new FirstCharEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = user.UserId,
                    EntityType = firstCharsEntityType,
                    FirstChars = lastNameFirstChar,
                    FollowersCount = 1,
                });
            }
        }
        else
        {
            var delta = increase ? 1 : -1;
            firstCharEntity.FollowersCount += delta;
            if (firstCharEntity.FollowersCount == 0)
            {
                await firstCharMapContext.Delete(user.UserId, lastNameFirstChar, firstCharsEntityType);
            }
            else
            {
                await firstCharMapContext.Update(firstCharEntity);
            }
        }
    }

    /// <summary>
    /// Gets all users followers by using FirstChar suffix.
    /// </summary>
    /// <param name="userId">In which user context to execute method, used to build partition key and chars count.</param>
    /// <returns>array of followers.</returns>
    private async Task<FirstCharSearchUserEntity[]> GetFlatFollowers(string userId)
    {
        var @params = await CreatePartitionKeyAndFirstCharParamsFromLastNameFirstCharsValue(userId);
        return [
            .. (
                    await Task.WhenAll(@params.Select(lastNameFirstCharContext.Get))
                ).SelectMany(items => items)
        ];
    }

    /// <summary>
    /// Create list of array params.
    /// </summary>
    /// <param name="userId">In which user context to execute method, used to build partition key and chars count.</param>
    /// <returns>Array of params.</returns>
    private async Task<IEnumerable<PartitionKeyAndFirstCharFilter>> CreatePartitionKeyAndFirstCharParamsFromLastNameFirstCharsValue(string userId)
    {
        var firstCharsValues = await firstCharMapContext.Get(userId, FirstCharsEntityType.LastName);
        return firstCharsValues.Select(firstCharsValue =>
        {
            var partitionKey = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars, _suffix);
            return new PartitionKeyAndFirstCharFilter(partitionKey, firstCharsValue.FirstChars);
        });
    }

    /// <summary>
    /// Create list of array params.
    /// </summary>
    /// <param name="userId">In which user context to execute method, used to build partition key and chars count.</param>
    /// <returns>Array of params.</returns>
    private async Task<IEnumerable<PartitionKeyAndFirstCharFilter>> CreatePartitionKeyAndFirstCharParamsFromFirstCharsFirstCharsValue(string userId)
    {
        var firstCharsValues = await firstCharMapContext.Get(userId, FirstCharsEntityType.FirstChars);
        return firstCharsValues
            .Select(firstCharsValue =>
            {
                var partitionKey1 = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars);
                var partitionKey2 = KeyHelper.CreateKeyByChars(userId, firstCharsValue.FirstChars, _suffix);
                PartitionKeyAndFirstCharFilter[] @params = [
                    new PartitionKeyAndFirstCharFilter(partitionKey1, firstCharsValue.FirstChars),
                    new PartitionKeyAndFirstCharFilter(partitionKey2, firstCharsValue.FirstChars),
                ];
                return @params;
            }).SelectMany(items => items);
    }
}
