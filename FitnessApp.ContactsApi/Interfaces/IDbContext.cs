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
    Task<MeFollowingEntity[]> Find(string followingId);
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
    Task<FirstCharSearchUserEntity> Get(PartitionKeyAndIdAndFirstCharFilter param);
    Task<FirstCharSearchUserEntity[]> Get(PartitionKeyAndIdAndFirstCharFilter[] @params);
    Task<FirstCharSearchUserEntity[]> Get(PartitionKeyAndFirstCharFilter param);
    Task<PagedDataModel<FirstCharSearchUserEntity>> Get(PartitionKeyAndFirstCharFilter param, GetUsersModel model);
    Task<FirstCharSearchUserEntity> Add(FirstCharSearchUserEntity user);
    Task Add(FirstCharSearchUserEntity[] users);
    Task<FirstCharSearchUserEntity> Update(FirstCharSearchUserEntity user);
    Task Replace(FirstCharSearchUserEntity[] users);
    Task Delete(PartitionKeyAndFirstCharFilter[] @params);
    Task<FirstCharSearchUserEntity> Delete(PartitionKeyAndIdAndFirstCharFilter param);
    Task Delete(PartitionKeyAndIdAndFirstCharFilter[] @params);
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
