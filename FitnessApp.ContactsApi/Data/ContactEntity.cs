using System;
using FitnessApp.Common.Abstractions.Db;
using FitnessApp.ContactsApi.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.ContactsApi.Data;

public interface IFollowerEntity : IWithUserIdEntity
{
    string FollowerId { get; set; }
}

public interface IFollowingEntity : IWithUserIdEntity
{
    string FollowingId { get; set; }
}

public interface IFirstChars
{
    string FirstChars { get; set; }
}

public interface IEntityType
{
    FirstCharsEntityType EntityType { get; set; }
}

public abstract class Entity : IGenericEntity
{
    [BsonId]
    public string Id { get; set; }
}

public abstract class WithUserIdEntity : Entity, IWithUserIdEntity
{
    public string UserId { get; set; }
}

public abstract class UserEntityBase : WithUserIdEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public byte Rating { get; set; }
    public byte Category { get; set; }
}

public abstract class SearchUserEntity : UserEntityBase, IPartitionKey
{
    public int CalculatedRating => Category * Rating;
    public string PartitionKey { get; set; }
}

public class UserEntity : UserEntityBase
{
    public int FollowersCount { get; set; }
    public DateTime CategoryDate { get; set; }
}

public class MyFollowerEntity : WithUserIdEntity, IFollowerEntity
{
    public string FollowerId { get; set; }
}

public class MeFollowingEntity : WithUserIdEntity, IFollowingEntity
{
    public string FollowingId { get; set; }
}

public class FollowRequestEntity : Entity
{
    public string ThisId { get; set; }
    public string OtherId { get; set; }
    public FollowRequestType RequestType { get; set; }
    public DateTime SentDateTime { get; set; }
}

public class FirstCharSearchUserEntity : SearchUserEntity, IFirstChars
{
    public string FirstChars { get; set; }
}

public class FirstCharEntity : WithUserIdEntity, IFirstChars, IEntityType
{
    public string FirstChars { get; set; }
    public FirstCharsEntityType EntityType { get; set; }
    public int FollowersCount { get; set; }
}

public enum FirstCharsEntityType
{
    LastName,
    FirstChars
}

public class PartitionKeyAndFirstCharFilter(string partitionKey, string firstChars) :
    IMultipleParamFilter,
    IPartitionKey,
    IFirstChars
{
    public string PartitionKey { get; set; } = partitionKey;
    public string FirstChars { get; set; } = firstChars;
}

public class PartitionKeyAndIdAndFirstCharFilter(string partitionKey, string userId, string firstChars) :
    IMultipleParamFilter,
    IPartitionKey,
    IUserId,
    IFirstChars
{
    public string PartitionKey { get; set; } = partitionKey;
    public string UserId { get; set; } = userId;
    public string FirstChars { get; set; } = firstChars;
}
