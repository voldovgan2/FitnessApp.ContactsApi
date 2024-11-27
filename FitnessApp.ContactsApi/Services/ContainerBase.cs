using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public abstract class ContainerBase
{
    protected readonly IFirstCharSearchUserDbContext FirstCharsContext;

    protected ContainerBase(IFirstCharSearchUserDbContext firstCharsContext)
    {
        FirstCharsContext = firstCharsContext;
    }

    protected Task AddUserToFirstCharsContext(
        UserEntity userToAdd,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToAdd, 0, charsCount);
        var users = firstCharsKeys.Select(firstCharsKey =>
        {
            return ConvertHelper.FirstCharSearchUserEntityFromUserEntity(
                userToAdd,
                firstCharsKey,
                createPartitionKey(firstCharsKey));
        });
        return FirstCharsContext.Add([..users]);
    }

    protected Task RemoveUserFromFirstCharsContext(
        UserEntity userToRemove,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToRemove, 0, charsCount);
        var @params = firstCharsKeys.Select(firstCharsKey => new PartitionKeyAndIdAndFirstCharFilter(
            createPartitionKey(firstCharsKey),
            userToRemove.UserId,
            firstCharsKey
        ));
        return FirstCharsContext.Delete([..@params]);
    }

    /// <summary>
    /// Updates user general info(for now just artificial rating) in search context, not affects partition key and chars count.
    /// </summary>
    /// <param name="user">User to update.</param>
    /// <param name="createPartitionKey">Func to create partition key for foltering only.</param>
    /// <param name="charsCount">Chars count.</param>
    /// <returns>Task.</returns>
    protected async Task UpdateUserInFirstCharsContext(
        UserEntity user,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var keysToUpdate = KeyHelper.GetKeysByFirstChars(user, 0, charsCount);
        var @params = keysToUpdate
            .Select(keyToUpdate => new PartitionKeyAndIdAndFirstCharFilter(createPartitionKey(keyToUpdate), user.UserId, keyToUpdate))
            .ToArray();
        var firstCharSearchUserEntities = await FirstCharsContext.Get(@params);
        foreach (var firstCharSearchUserEntity in firstCharSearchUserEntities)
        {
            ConvertHelper.UpdateFirstCharSearchUserEntityByUserEntity(firstCharSearchUserEntity, user);
        }

        await FirstCharsContext.Replace(firstCharSearchUserEntities);
    }

    protected async Task UpdateUserInFirstCharsContext(
        UserEntity oldUser,
        UserEntity newUser,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var (KeysToRemove, KeysToAdd) = KeyHelper.GetUnMatchedKeys(oldUser, newUser, charsCount);
        var @params = KeysToRemove.Select(keyToRemove => new PartitionKeyAndIdAndFirstCharFilter(
            createPartitionKey(keyToRemove),
            oldUser.UserId,
            keyToRemove
        ));
        var deleteUsersTask = FirstCharsContext.Delete([..@params]);

        var users = KeysToAdd.Select(keyToAdd => ConvertHelper.FirstCharSearchUserEntityFromUserEntity(
                newUser,
                keyToAdd,
                createPartitionKey(keyToAdd)));
        var addUsersTask = FirstCharsContext.Add([.. users]);

        await Task.WhenAll(deleteUsersTask, addUsersTask);
    }

    protected async Task<PagedDataModel<SearchUserEntity>> GetUserFromFirstCharsContext(
        GetUsersModel model,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var chars = KeyHelper.GetSubstring(model.Search, charsCount);
        var partitionKey = createPartitionKey(chars);
        var data = await FirstCharsContext.Get(new PartitionKeyAndFirstCharFilter(partitionKey, chars), model);
        return new PagedDataModel<SearchUserEntity>
        {
            Page = data.Page,
            TotalCount = data.TotalCount,
            Items = data.Items,
        };
    }
}
