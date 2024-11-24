using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Enums;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IUserDbContext
{
    Task<UserEntity> Get(string userId);
    Task<UserEntity> Add(UserEntity entity);
    Task<UserEntity> UpdateUser(UserEntity entity);
    Task<UserEntity> DeleteUser(string userId);
}

public interface IFollowerDbContext
{
    Task<MyFollowerEntity?> Find(string userId, string followerId);
    Task<MyFollowerEntity[]> Find(string userId);
    Task<MyFollowerEntity> Add(MyFollowerEntity entity);
    Task<MyFollowerEntity?> Delete(string userId, string followerId);
}

public interface IFollowingDbContext
{
    Task<MeFollowingEntity?> Find(string userId, string followingId);
    Task<MeFollowingEntity[]> Find(string userId);
    Task<MeFollowingEntity> Add(MeFollowingEntity entity);
    Task<MeFollowingEntity?> Delete(string userId, string followingId);
}

public interface IFollowerRequestDbContext
{
    Task<PagedDataModel<FollowRequestEntity>> Get(
        string thisId,
        FollowRequestType followRequestType,
        GetUsersModel model);
    Task<FollowRequestEntity> Add(string thisId, string otherId);
    Task<FollowRequestEntity> Delete(string thisId, string otherId);
}

public interface IFirstCharSearchUserDbContext
{
    Task<FirstCharSearchUserEntity> Get(
        string partitionKey,
        string userId,
        string firstChars);
    Task<FirstCharSearchUserEntity[]> Get(
        string partitionKey,
        string firstChars);
    Task<PagedDataModel<FirstCharSearchUserEntity>> Get(
        string partitionKey,
        string firstChars,
        GetUsersModel model);
    Task<FirstCharSearchUserEntity> Add(FirstCharSearchUserEntity user);
    Task Add(FirstCharSearchUserEntity[] users);
    Task<FirstCharSearchUserEntity> Update(FirstCharSearchUserEntity user);
    Task Delete((string PartitionKey, string FirstChars)[] @params);
    Task<FirstCharSearchUserEntity> Delete(
        string partitionKey,
        string userId,
        string firstChars);
    Task Delete((
        string PartitionKey,
        string UserId,
        string FirstChars)[] @params);
}

public interface IFirstCharDbContext
{
    Task<FirstCharEntity?> TryGet(string userId, string firstChars, FirstCharsEntityType entityType);
    Task<FirstCharEntity> Get(string userId, string firstChars, FirstCharsEntityType entityType);
    Task<FirstCharEntity[]> Get(string userId, FirstCharsEntityType entityType);
    Task<FirstCharEntity> Add(FirstCharEntity entity);
    Task<FirstCharEntity> Update(FirstCharEntity entity);
    Task<FirstCharEntity> Delete(
        string userId,
        string firstChars,
        FirstCharsEntityType entityType);
}
