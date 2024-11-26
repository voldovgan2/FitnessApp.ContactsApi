using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Enums;
using FitnessApp.ContactsApi.Exceptions;
using FitnessApp.ContactsApi.Helpers;
using FitnessApp.ContactsApi.Interfaces;
using FitnessApp.ContactsApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FitnessApp.ContactsApi.Services;

public class UserDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContextBase<UserEntity>(mongoClient, snapshot.Get("User")),
    IUserDbContext
{
    public async Task<UserEntity> Get(string userId)
    {
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            DbContextHelper.CreateGetByUserIdFiter<UserEntity>(userId));
        return items.FirstOrDefault() ?? throw new EntityNotFoundException(Settings.CollecttionName, userId);
    }

    public async Task<UserEntity> Add(UserEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return await Get(entity.UserId);
    }

    public async Task<UserEntity> UpdateUser(UserEntity entity)
    {
        var replaceResult = await Collection.ReplaceOneAsync(DbContextHelper.CreateGetByUserIdFiter<UserEntity>(entity.UserId), entity);
        return Common.Helpers.DbContextHelper.IsConfirmed(replaceResult.IsAcknowledged, replaceResult.MatchedCount)
            ? await Get(entity.UserId)
            : throw new EntityNotFoundException(Settings.CollecttionName, entity.UserId);
    }

    public async Task<UserEntity> DeleteUser(string userId)
    {
        var deleted = await Get(userId);
        var deleteResult = await Collection.DeleteOneAsync(DbContextHelper.CreateGetByUserIdFiter<UserEntity>(userId));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new EntityNotFoundException(Settings.CollecttionName, userId);
    }
}

public class FollowerDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContext<MyFollowerEntity>(mongoClient, snapshot.Get("Follower")),
    IFollowerDbContext
{
    public Task<MyFollowerEntity> Add(MyFollowerEntity entity)
    {
        return CreateItem(entity);
    }

    public async Task<MyFollowerEntity?> Find(string userId, string followerId)
    {
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            DbContextHelper.CreateGetByUserIdAndFollowerIdFiter<MyFollowerEntity>(userId, followerId));
        return items.FirstOrDefault();
    }

    public Task<MyFollowerEntity[]> Find(string userId)
    {
        return Common.Helpers.DbContextHelper
            .FilterCollection(
                Collection,
                DbContextHelper.CreateGetByUserIdFiter<MyFollowerEntity>(userId));
    }

    public async Task<MyFollowerEntity> Delete(string userId, string followerId)
    {
        var result = await Find(userId, followerId) ?? throw new FollowerNotFoundException(userId, followerId);
        var deleteResult = await Collection.DeleteOneAsync(DbContextHelper.CreateGetByUserIdFiter<MyFollowerEntity>(userId));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? result
            : throw new FollowerNotFoundException(userId, followerId);
    }
}

public class FollowingDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContext<MeFollowingEntity>(mongoClient, snapshot.Get("Following")),
    IFollowingDbContext
{
    public Task<MeFollowingEntity> Add(MeFollowingEntity entity)
    {
        return CreateItem(entity);
    }

    public async Task<MeFollowingEntity?> Find(string userId, string followingId)
    {
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            DbContextHelper.CreateGetByUserIdAndFollowingIdFiter<MeFollowingEntity>(userId, followingId));
        return items.FirstOrDefault();
    }

    public Task<MeFollowingEntity[]> Find(string userId)
    {
        return Common.Helpers.DbContextHelper
            .FilterCollection(
                Collection,
                DbContextHelper.CreateGetByUserIdFiter<MeFollowingEntity>(userId));
    }

    public async Task<MeFollowingEntity> Delete(string userId, string followingId)
    {
        var result = await Find(userId, followingId) ?? throw new FollowerNotFoundException(userId, followingId);
        var deleteResult = await Collection.DeleteOneAsync(DbContextHelper.CreateGetByUserIdFiter<MeFollowingEntity>(userId));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? result
            : throw new FollowerNotFoundException(userId, followingId);
    }
}

public class FollowerRequestDbContext(
    IMongoClient mongoClient,
    IOptionsSnapshot<MongoDbSettings> snapshot,
    IDateTimeService dateTimeService) :
    DbContextBase<FollowRequestEntity>(mongoClient, snapshot.Get("FollowerRequest")),
    IFollowerRequestDbContext
{
    public Task<PagedDataModel<FollowRequestEntity>> Get(
        string thisId,
        FollowRequestType followRequestType,
        GetUsersModel model)
    {
        return Common.Helpers.DbContextHelper.GetPagedCollection(
            Collection,
            CreateGetByThisIdAndFollowRequestTypeFiter(thisId, followRequestType),
            model);
    }

    public async Task<FollowRequestEntity> Add(string thisId, string otherId)
    {
        await Collection.InsertOneAsync(new FollowRequestEntity
        {
            ThisId = thisId,
            OtherId = otherId,
            RequestType = FollowRequestType.In,
            SentDateTime = dateTimeService.Now,
        });
        return await Get(thisId, otherId);
    }

    public async Task<FollowRequestEntity> Delete(string thisId, string otherId)
    {
        var deleted = await Get(thisId, otherId);
        var deleteResult = await Collection.DeleteOneAsync(CreateGetByThisIdAndOtherIdFiter(thisId, otherId));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FollowerRequestNotFoundException(thisId, otherId);
    }

    private async Task<FollowRequestEntity> Get(string thisId, string otherId)
    {
        var items = await Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            CreateGetByThisIdAndOtherIdFiter(thisId, otherId));
        return items.FirstOrDefault() ?? throw new FollowerRequestNotFoundException(thisId, otherId);
    }

    public static FilterDefinition<FollowRequestEntity> CreateGetByThisIdAndOtherIdFiter(string thisId, string otherId)
    {
        return Builders<FollowRequestEntity>
                        .Filter
                        .And(
                            CreateGetByThisIdFiter(thisId),
                            CreateGetByOtherIdFiter(otherId));
    }

    public static FilterDefinition<FollowRequestEntity> CreateGetByThisIdAndFollowRequestTypeFiter(string thisId, FollowRequestType followRequestType)
    {
        return Builders<FollowRequestEntity>
                        .Filter
                        .And(
                            CreateGetByThisIdFiter(thisId),
                            CreateGetByFollowRequestTypeFiter(followRequestType));
    }

    public static FilterDefinition<FollowRequestEntity> CreateGetByThisIdFiter(string thisId)
    {
        return Builders<FollowRequestEntity>.Filter.Eq(s => s.ThisId, thisId);
    }

    public static FilterDefinition<FollowRequestEntity> CreateGetByOtherIdFiter(string otherId)
    {
        return Builders<FollowRequestEntity>.Filter.Eq(s => s.OtherId, otherId);
    }

    public static FilterDefinition<FollowRequestEntity> CreateGetByFollowRequestTypeFiter(FollowRequestType followRequestType)
    {
        return Builders<FollowRequestEntity>.Filter.Eq(s => s.RequestType, followRequestType);
    }
}

public class FirstCharSearchUserDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContextBase<FirstCharSearchUserEntity>(mongoClient, snapshot.Get("FirstCharSearchUser")),
    IFirstCharSearchUserDbContext
{
    public async Task<FirstCharSearchUserEntity> Get(PartitionKeyAndIdAndFirstCharFilter param)
    {
        var result = await Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            CreateGetByPartitionAndIdAndFirstCharsFiter(param));
        return
            result.FirstOrDefault() ??
            throw new FirstCharSearchUserNotFoundException(
                param.PartitionKey,
                param.UserId,
                param.FirstChars);
    }

    public Task<FirstCharSearchUserEntity[]> Get(PartitionKeyAndFirstCharFilter param)
    {
        return Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            CreateGetByPartitionAndFirstCharsFiter(param));
    }

    public Task<PagedDataModel<FirstCharSearchUserEntity>> Get(PartitionKeyAndFirstCharFilter param, GetUsersModel model)
    {
        return Common.Helpers.DbContextHelper.GetPagedCollection(
            Collection,
            CreateGetByPartitionAndFirstCharsFiter(param),
            model);
    }

    public async Task<FirstCharSearchUserEntity> Add(FirstCharSearchUserEntity user)
    {
        await Collection.InsertOneAsync(user);
        return await Get(new PartitionKeyAndIdAndFirstCharFilter(user.PartitionKey, user.UserId, user.FirstChars));
    }

    public Task Add(FirstCharSearchUserEntity[] users)
    {
        HashSet<string> ids = new HashSet<string>();
        foreach (var item in users)
        {
            ids.Add(item.Id);
        }

        return Collection.InsertManyAsync(users);
    }

    public async Task<FirstCharSearchUserEntity> Update(FirstCharSearchUserEntity user)
    {
        var param = new PartitionKeyAndIdAndFirstCharFilter(user.PartitionKey, user.UserId, user.FirstChars);
        await Collection.ReplaceOneAsync(CreateGetByPartitionAndIdAndFirstCharsFiter(param), user);
        return await Get(param);
    }

    public async Task Delete(PartitionKeyAndFirstCharFilter[] @params)
    {
        await Collection.DeleteManyAsync(Common.Helpers.DbContextHelper.CreateGetByArrayParamsFiter(@params, CreateGetByPartitionAndFirstCharsFiter));
    }

    public async Task<FirstCharSearchUserEntity> Delete(PartitionKeyAndIdAndFirstCharFilter param)
    {
        var deleted = await Get(param);
        var deleteResult = await Collection.DeleteOneAsync(CreateGetByPartitionAndIdAndFirstCharsFiter(param));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FirstCharSearchUserNotFoundException(
                param.PartitionKey,
                param.UserId,
                param.FirstChars);
    }

    public async Task Delete(PartitionKeyAndIdAndFirstCharFilter[] @params)
    {
        await Collection.DeleteManyAsync(Common.Helpers.DbContextHelper.CreateGetByArrayParamsFiter(@params, CreateGetByPartitionAndIdAndFirstCharsFiter));
    }

    private static FilterDefinition<FirstCharSearchUserEntity> CreateGetByPartitionAndFirstCharsFiter(PartitionKeyAndFirstCharFilter param)
    {
        return Builders<FirstCharSearchUserEntity>
            .Filter
            .And(
                Common.Helpers.DbContextHelper.CreateGetByPartitionKeyFiter<FirstCharSearchUserEntity>(param.PartitionKey),
                DbContextHelper.CreateGetByFirstCharsFiter<FirstCharSearchUserEntity>(param.FirstChars));
    }

    private static FilterDefinition<FirstCharSearchUserEntity> CreateGetByPartitionAndIdAndFirstCharsFiter(PartitionKeyAndIdAndFirstCharFilter param)
    {
        return Builders<FirstCharSearchUserEntity>
            .Filter
            .And(
                Common.Helpers.DbContextHelper.CreateGetByPartitionKeyFiter<FirstCharSearchUserEntity>(param.PartitionKey),
                DbContextHelper.CreateGetByUserIdFiter<FirstCharSearchUserEntity>(param.UserId),
                DbContextHelper.CreateGetByFirstCharsFiter<FirstCharSearchUserEntity>(param.FirstChars));
    }
}

public class FirstCharDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContextBase<FirstCharEntity>(mongoClient, snapshot.Get("FirstChar")),
    IFirstCharDbContext
{
    public async Task<FirstCharEntity?> TryGet(string userId, string firstChars, FirstCharsEntityType entityType)
    {
        var items = await Common.Helpers.DbContextHelper.FilterCollection(Collection, CreateGetByIdAndEntityTypeAndFirstCharsFiter(userId, entityType, firstChars));
        return items.FirstOrDefault();
    }

    public async Task<FirstCharEntity> Get(string userId, string firstChars, FirstCharsEntityType entityType)
    {
        var item = await TryGet(userId, firstChars, entityType);
        return item ?? throw new FirstCharEntityNotFoundException(userId, firstChars, entityType);
    }

    public Task<FirstCharEntity[]> Get(string userId, FirstCharsEntityType entityType)
    {
        return Common.Helpers.DbContextHelper.FilterCollection(Collection, CreateGetByIdAndEntityTypeFiter(userId, entityType));
    }

    public async Task<FirstCharEntity> Add(FirstCharEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return await Get(entity.UserId, entity.FirstChars, entity.EntityType);
    }

    public async Task<FirstCharEntity> Update(FirstCharEntity entity)
    {
        await Collection.ReplaceOneAsync(
            CreateGetByIdAndEntityTypeAndFirstCharsFiter(
                entity.UserId,
                entity.EntityType,
                entity.FirstChars),
            entity);
        return await Get(entity.UserId, entity.FirstChars, entity.EntityType);
    }

    public async Task<FirstCharEntity> Delete(string userId, string firstChars, FirstCharsEntityType entityType)
    {
        var deleted = await Get(userId, firstChars, entityType) ?? throw new FirstCharEntityNotFoundException(userId, firstChars, entityType);
        var deleteResult = await Collection.DeleteOneAsync(CreateGetByIdAndEntityTypeAndFirstCharsFiter(userId, entityType, firstChars));
        return Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FirstCharEntityNotFoundException(userId, firstChars, entityType);
    }

    private static FilterDefinition<FirstCharEntity> CreateGetByIdAndEntityTypeFiter(string userId, FirstCharsEntityType entityType)
    {
        return Builders<FirstCharEntity>
            .Filter
            .And(
                DbContextHelper.CreateGetByEntityTypeFiter<FirstCharEntity>(entityType),
                DbContextHelper.CreateGetByUserIdFiter<FirstCharEntity>(userId));
    }

    private static FilterDefinition<FirstCharEntity> CreateGetByIdAndEntityTypeAndFirstCharsFiter(string userId, FirstCharsEntityType entityType, string firstChars)
    {
        return Builders<FirstCharEntity>
            .Filter
            .And(
                DbContextHelper.CreateGetByUserIdFiter<FirstCharEntity>(userId),
                DbContextHelper.CreateGetByEntityTypeFiter<FirstCharEntity>(entityType),
                DbContextHelper.CreateGetByFirstCharsFiter<FirstCharEntity>(firstChars));
    }
}
