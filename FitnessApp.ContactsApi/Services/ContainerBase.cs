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

    protected async Task AddUserToFirstCharsContext(
        UserEntity userToAdd,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToAdd, 0, charsCount);
        var addTasks = firstCharsKeys.Select(firstCharsKey =>
        {
            var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(userToAdd);
            firstCharSearchUser.Id = Guid.NewGuid().ToString("N");
            firstCharSearchUser.FirstChars = firstCharsKey;
            firstCharSearchUser.PartitionKey = createPartitionKey(firstCharsKey);
            return FirstCharsContext.Add(firstCharSearchUser);
        });
        await Task.WhenAll(addTasks);
    }

    protected async Task RemoveUserFromFirstCharsContext(
        UserEntity userToRemove,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var firstCharsKeys = KeyHelper.GetKeysByFirstChars(userToRemove, 0, charsCount);
        var deleteTasks = firstCharsKeys.Select(firstCharsKey =>
        {
            return FirstCharsContext.Delete(
                createPartitionKey(firstCharsKey),
                userToRemove.UserId,
                firstCharsKey);
        });
        await Task.WhenAll(deleteTasks);
    }

    protected async Task UpdateUserInFirstCharsContext(
        UserEntity user,
        Func<string, string> createPartitionKey,
        int charsCount)
    {
        var keysToUpdate = KeyHelper.GetKeysByFirstChars(user, 0, charsCount);
        var updateTasks = keysToUpdate.Select(keyToUpdate =>
        {
            var firstCharSearchUserEntityTask = FirstCharsContext.Get(createPartitionKey(keyToUpdate), user.UserId, keyToUpdate);
            return firstCharSearchUserEntityTask.ContinueWith((firstCharSearchUserEntity) =>
            {
                firstCharSearchUserEntity.Result.PartitionKey = createPartitionKey(keyToUpdate);
                firstCharSearchUserEntity.Result.FirstChars = keyToUpdate;
                return FirstCharsContext.Update(firstCharSearchUserEntity.Result);
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
        var deleteTasks = KeysToRemove.Select(keyToRemove =>
        {
            var partitionKey = createPartitionKey(keyToRemove);
            return FirstCharsContext.Delete(partitionKey, oldUser.UserId, keyToRemove);
        });
        await Task.WhenAll(deleteTasks);

        var firstCharSearchUser = Mapper.Map<FirstCharSearchUserEntity>(newUser);
        var addTasks = KeysToAdd.Select(keyToAdd =>
        {
            var partitionKey = createPartitionKey(keyToAdd);
            firstCharSearchUser.PartitionKey = partitionKey;
            firstCharSearchUser.FirstChars = keyToAdd;
            return FirstCharsContext.Add(firstCharSearchUser);
        });
        await Task.WhenAll(addTasks);
    }

    protected async Task<PagedDataModel<SearchUserEntity>> GetUserFromFirstCharsContext(GetUsersModel model, int charsCount)
    {
        var chars = KeyHelper.GetSubstring(model.Search, 0, charsCount);
        var partitionKey = KeyHelper.CreateKeyByChars(chars);
        var data = await FirstCharsContext.Get(partitionKey, chars, model);
        return new PagedDataModel<SearchUserEntity>
        {
            Page = data.Page,
            TotalCount = data.TotalCount,
            Items = data.Items,
        };
    }
}
