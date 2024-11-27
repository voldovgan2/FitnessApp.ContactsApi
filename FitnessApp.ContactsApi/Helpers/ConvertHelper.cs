using System;
using System.Linq;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Helpers;

public static class ConvertHelper
{
    public static PagedDataModel<UserModel> PagedFirstCharSearchUserEntityFromPagedUserModel(PagedDataModel<SearchUserEntity> searchUserEntities)
    {
        return new PagedDataModel<UserModel>
        {
            Page = searchUserEntities.Page,
            TotalCount = searchUserEntities.TotalCount,
            Items = searchUserEntities.Items.Select(item => new UserModel
            {
                UserId = item.UserId,
                FirstName = item.FirstName,
                LastName = item.LastName,
            }).ToArray(),
        };
    }

    public static UserEntity UserEntityFromUserModel(UserModel userModel)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userModel.UserId,
            FirstName = userModel.FirstName,
            LastName = userModel.LastName,
        };
    }

    public static FirstCharSearchUserEntity FirstCharSearchUserEntityFromUserEntity(
        UserEntity userEntity,
        string firstChars,
        string partitionKey)
    {
        return new FirstCharSearchUserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = userEntity.UserId,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Category = userEntity.Category,
            Rating = userEntity.Rating,
            FirstChars = firstChars,
            PartitionKey = partitionKey,
        };
    }

    public static FirstCharSearchUserEntity FirstCharSearchUserEntityFromFirstCharSearchUserEntity(
        FirstCharSearchUserEntity firstCharSearchUserEntity,
        string firstChars,
        string partitionKey)
    {
        return new FirstCharSearchUserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = firstCharSearchUserEntity.UserId,
            FirstName = firstCharSearchUserEntity.FirstName,
            LastName = firstCharSearchUserEntity.LastName,
            Category = firstCharSearchUserEntity.Category,
            Rating = firstCharSearchUserEntity.Rating,
            FirstChars = firstChars,
            PartitionKey = partitionKey,
        };
    }

    public static void UpdateFirstCharSearchUserEntityByUserEntity(FirstCharSearchUserEntity firstCharSearchUserEntity, UserEntity userEntity)
    {
        firstCharSearchUserEntity.Rating = userEntity.Rating;
    }
}
