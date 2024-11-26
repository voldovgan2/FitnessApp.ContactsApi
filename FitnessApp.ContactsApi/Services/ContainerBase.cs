using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Services;

public abstract class ContainerBase
{
    protected readonly IMapper Mapper;
    protected readonly IFirstCharSearchUserDbContext FirstCharsContext;

    protected ContainerBase(IMapper mapper, IFirstCharSearchUserDbContext firstCharsContext)
    {
        Mapper = mapper;
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
            var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToAdd);
            firstCharSearchUser.Id = Guid.NewGuid().ToString("N");
            firstCharSearchUser.FirstChars = firstCharsKey;
            firstCharSearchUser.PartitionKey = createPartitionKey(firstCharsKey);
            return firstCharSearchUser;
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

    protected async Task UpdateUserInFirstCharsContext(
        UserEntity user,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var keysToUpdate = KeyHelper.GetKeysByFirstChars(user, 0, charsCount);
        var updateTasks = keysToUpdate.Select(keyToUpdate =>
        {
            var getFirstCharSearchUserEntityTask = FirstCharsContext.Get(new PartitionKeyAndIdAndFirstCharFilter(createPartitionKey(keyToUpdate), user.UserId, keyToUpdate));
            return getFirstCharSearchUserEntityTask.ContinueWith((firstCharSearchUserEntityTask) =>
            {
                var firstCharSearchUserEntity = firstCharSearchUserEntityTask.Result;
                firstCharSearchUserEntity.Category = user.Category;
                firstCharSearchUserEntity.PartitionKey = createPartitionKey(keyToUpdate);
                firstCharSearchUserEntity.FirstChars = keyToUpdate;
                return FirstCharsContext.Update(firstCharSearchUserEntity);
            });
        });
        await Task.WhenAll(updateTasks);
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

        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(newUser);
        var users = KeysToAdd.Select(keyToAdd =>
        {
            var partitionKey = createPartitionKey(keyToAdd);
            firstCharSearchUser.PartitionKey = partitionKey;
            firstCharSearchUser.FirstChars = keyToAdd;
            return firstCharSearchUser;
        });
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
