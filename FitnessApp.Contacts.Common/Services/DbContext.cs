using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Common.Exceptions;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Enums;
using FitnessApp.Contacts.Common.Exceptions;
using FitnessApp.Contacts.Common.Interfaces;
using FitnessApp.Contacts.Common.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FitnessApp.Contacts.Common.Services;

public class UserDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContextBase<UserEntity>(mongoClient, snapshot.Get("User")),
    IUserDbContext
{
    public async Task<UserEntity> Get(string userId)
    {
        var items = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<UserEntity>(userId));
        return items.FirstOrDefault() ?? throw new EntityNotFoundException(Settings.CollecttionName, userId);
    }

    public async Task<UserEntity> Add(UserEntity entity)
    {
        await Collection.InsertOneAsync(entity);
        return await Get(entity.UserId);
    }

    public async Task<UserEntity> UpdateUser(UserEntity entity)
    {
        var replaceResult = await Collection.ReplaceOneAsync(FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<UserEntity>(entity.UserId), entity);
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(replaceResult.IsAcknowledged, replaceResult.MatchedCount)
            ? await Get(entity.UserId)
            : throw new EntityNotFoundException(Settings.CollecttionName, entity.UserId);
    }

    public async Task<UserEntity> DeleteUser(string userId)
    {
        var deleted = await Get(userId);
        var deleteResult = await Collection.DeleteOneAsync(FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<UserEntity>(userId));
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
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
        var items = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            Helpers.DbContextHelper.CreateGetByUserIdAndFollowerIdFiter<MyFollowerEntity>(userId, followerId));
        return items.FirstOrDefault();
    }

    public Task<MyFollowerEntity[]> Find(string userId)
    {
        return FitnessApp.Common.Helpers.DbContextHelper
            .FilterCollection(
                Collection,
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<MyFollowerEntity>(userId));
    }

    public async Task<MyFollowerEntity> Delete(string userId, string followerId)
    {
        var result = await Find(userId, followerId) ?? throw new FollowerNotFoundException(userId, followerId);
        var deleteResult = await Collection.DeleteOneAsync(Helpers.DbContextHelper.CreateGetByUserIdAndFollowerIdFiter<MyFollowerEntity>(userId, followerId));
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
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

    public async Task<MeFollowingEntity> Get(string userId, string followingId)
    {
        var items = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            Helpers.DbContextHelper.CreateGetByUserIdAndFollowingIdFiter<MeFollowingEntity>(userId, followingId));
        return items.FirstOrDefault() ?? throw new FollowerNotFoundException(userId, followingId);
    }

    public Task<MeFollowingEntity[]> Find(string followingId)
    {
        return FitnessApp.Common.Helpers.DbContextHelper
            .FilterCollection(
                Collection,
                CreateGetByFollowingIdFiter(followingId));
    }

    public async Task<MeFollowingEntity> Delete(string userId, string followingId)
    {
        var result = await Get(userId, followingId) ?? throw new FollowerNotFoundException(userId, followingId);
        var deleteResult = await Collection.DeleteOneAsync(Helpers.DbContextHelper.CreateGetByUserIdAndFollowingIdFiter<MeFollowingEntity>(userId, followingId));
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? result
            : throw new FollowerNotFoundException(userId, followingId);
    }

    private static FilterDefinition<MeFollowingEntity> CreateGetByFollowingIdFiter(string followingId)
    {
        return Builders<MeFollowingEntity>
            .Filter
            .Eq(s => s.FollowingId, followingId);
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
        return FitnessApp.Common.Helpers.DbContextHelper.GetPagedCollection(
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
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FollowerRequestNotFoundException(thisId, otherId);
    }

    private async Task<FollowRequestEntity> Get(string thisId, string otherId)
    {
        var items = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
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
        var result = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            CreateGetByPartitionAndIdAndFirstCharsFiter(param));
        return
            result.FirstOrDefault() ??
            throw new FirstCharSearchUserNotFoundException(
                param.PartitionKey,
                param.UserId,
                param.FirstChars);
    }

    public async Task<FirstCharSearchUserEntity[]> Get(PartitionKeyAndIdAndFirstCharFilter[] @params)
    {
        return await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            FitnessApp.Common.Helpers.DbContextHelper.CreateGetByArrayParamsFiter(@params, CreateGetByPartitionAndIdAndFirstCharsFiter));
    }

    public Task<FirstCharSearchUserEntity[]> Get(PartitionKeyAndFirstCharFilter param)
    {
        return FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(
            Collection,
            CreateGetByPartitionAndFirstCharsFiter(param));
    }

    public Task<PagedDataModel<FirstCharSearchUserEntity>> Get(PartitionKeyAndFirstCharFilter param, GetUsersModel model)
    {
        return FitnessApp.Common.Helpers.DbContextHelper.GetPagedCollection(
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
        return Collection.InsertManyAsync(users);
    }

    public async Task<FirstCharSearchUserEntity> Update(FirstCharSearchUserEntity user)
    {
        var param = new PartitionKeyAndIdAndFirstCharFilter(user.PartitionKey, user.UserId, user.FirstChars);
        await Collection.ReplaceOneAsync(CreateGetByPartitionAndIdAndFirstCharsFiter(param), user);
        return await Get(param);
    }

    public async Task Replace(FirstCharSearchUserEntity[] users)
    {
        foreach (var user in users)
        {
            var param = new PartitionKeyAndIdAndFirstCharFilter(user.PartitionKey, user.UserId, user.FirstChars);
            await Collection.ReplaceOneAsync(CreateGetByPartitionAndIdAndFirstCharsFiter(param), user);
        }
    }

    public async Task Delete(PartitionKeyAndFirstCharFilter[] @params)
    {
        await Collection.DeleteManyAsync(FitnessApp.Common.Helpers.DbContextHelper.CreateGetByArrayParamsFiter(@params, CreateGetByPartitionAndFirstCharsFiter));
    }

    public async Task<FirstCharSearchUserEntity> Delete(PartitionKeyAndIdAndFirstCharFilter param)
    {
        var deleted = await Get(param);
        var deleteResult = await Collection.DeleteOneAsync(CreateGetByPartitionAndIdAndFirstCharsFiter(param));
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FirstCharSearchUserNotFoundException(
                param.PartitionKey,
                param.UserId,
                param.FirstChars);
    }

    public async Task Delete(PartitionKeyAndIdAndFirstCharFilter[] @params)
    {
        await Collection.DeleteManyAsync(FitnessApp.Common.Helpers.DbContextHelper.CreateGetByArrayParamsFiter(@params, CreateGetByPartitionAndIdAndFirstCharsFiter));
    }

    private static FilterDefinition<FirstCharSearchUserEntity> CreateGetByPartitionAndFirstCharsFiter(PartitionKeyAndFirstCharFilter param)
    {
        return Builders<FirstCharSearchUserEntity>
            .Filter
            .And(
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByPartitionKeyFiter<FirstCharSearchUserEntity>(param.PartitionKey),
                Helpers.DbContextHelper.CreateGetByFirstCharsFiter<FirstCharSearchUserEntity>(param.FirstChars));
    }

    private static FilterDefinition<FirstCharSearchUserEntity> CreateGetByPartitionAndIdAndFirstCharsFiter(PartitionKeyAndIdAndFirstCharFilter param)
    {
        return Builders<FirstCharSearchUserEntity>
            .Filter
            .And(
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByPartitionKeyFiter<FirstCharSearchUserEntity>(param.PartitionKey),
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<FirstCharSearchUserEntity>(param.UserId),
                Helpers.DbContextHelper.CreateGetByFirstCharsFiter<FirstCharSearchUserEntity>(param.FirstChars));
    }
}

public class FirstCharDbContext(IMongoClient mongoClient, IOptionsSnapshot<MongoDbSettings> snapshot) :
    DbContextBase<FirstCharEntity>(mongoClient, snapshot.Get("FirstChar")),
    IFirstCharDbContext
{
    public async Task<FirstCharEntity?> TryGet(
        string userId,
        string firstChars,
        FirstCharsEntityType entityType)
    {
        var items = await FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(Collection, CreateGetByIdAndEntityTypeAndFirstCharsFiter(userId, entityType, firstChars));
        return items.FirstOrDefault();
    }

    public async Task<FirstCharEntity> Get(
        string userId,
        string firstChars,
        FirstCharsEntityType entityType)
    {
        var item = await TryGet(userId, firstChars, entityType);
        return item ?? throw new FirstCharEntityNotFoundException(
            userId,
            firstChars,
            entityType);
    }

    public Task<FirstCharEntity[]> Get(string userId, FirstCharsEntityType entityType)
    {
        return FitnessApp.Common.Helpers.DbContextHelper.FilterCollection(Collection, CreateGetByIdAndEntityTypeFiter(userId, entityType));
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

    public async Task<FirstCharEntity> Delete(
        string userId,
        string firstChars,
        FirstCharsEntityType entityType)
    {
        var deleted = await Get(userId, firstChars, entityType) ?? throw new FirstCharEntityNotFoundException(userId, firstChars, entityType);
        var deleteResult = await Collection.DeleteOneAsync(CreateGetByIdAndEntityTypeAndFirstCharsFiter(
            userId,
            entityType,
            firstChars));
        return FitnessApp.Common.Helpers.DbContextHelper.IsConfirmed(deleteResult.IsAcknowledged, deleteResult.DeletedCount)
            ? deleted
            : throw new FirstCharEntityNotFoundException(
                userId,
                firstChars,
                entityType);
    }

    private static FilterDefinition<FirstCharEntity> CreateGetByIdAndEntityTypeFiter(string userId, FirstCharsEntityType entityType)
    {
        return Builders<FirstCharEntity>
            .Filter
            .And(
                Helpers.DbContextHelper.CreateGetByEntityTypeFiter<FirstCharEntity>(entityType),
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<FirstCharEntity>(userId));
    }

    private static FilterDefinition<FirstCharEntity> CreateGetByIdAndEntityTypeAndFirstCharsFiter(
        string userId,
        FirstCharsEntityType entityType,
        string firstChars)
    {
        return Builders<FirstCharEntity>
            .Filter
            .And(
                FitnessApp.Common.Helpers.DbContextHelper.CreateGetByUserIdFiter<FirstCharEntity>(userId),
                Helpers.DbContextHelper.CreateGetByEntityTypeFiter<FirstCharEntity>(entityType),
                Helpers.DbContextHelper.CreateGetByFirstCharsFiter<FirstCharEntity>(firstChars));
    }
}
