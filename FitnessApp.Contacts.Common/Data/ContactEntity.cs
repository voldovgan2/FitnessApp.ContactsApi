﻿using FitnessApp.Common.Abstractions.Db;
using FitnessApp.Contacts.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.Contacts.Common.Data;

public interface IFollowerEntity : IWithUserIdEntity
{
    string FollowerId { get; init; }
}

public interface IFollowingEntity : IWithUserIdEntity
{
    string FollowingId { get; init; }
}

public interface IFirstChars
{
    string FirstChars { get; init; }
}

public interface IEntityType
{
    FirstCharsEntityType EntityType { get; init; }
}

public abstract class Entity : IGenericEntity
{
    [BsonId]
    public string Id { get; init; }
}

public abstract class WithUserIdEntity : Entity, IWithUserIdEntity
{
    public string UserId { get; init; }
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
    public string PartitionKey { get; init; }
}

public class UserEntity : UserEntityBase
{
    public int FollowersCount { get; set; }
    public DateTime CategoryDate { get; set; }
}

public class MyFollowerEntity : WithUserIdEntity, IFollowerEntity
{
    // UserId is partition key
    public string FollowerId { get; init; }
}

public class MeFollowingEntity : WithUserIdEntity, IFollowingEntity
{
    // UserId is partition key
    public string FollowingId { get; init; }
}

public class FollowRequestEntity : Entity
{
    public string ThisId { get; init; }
    public string OtherId { get; set; }
    public FollowRequestType RequestType { get; set; }
    public DateTime SentDateTime { get; set; }
}

public class FirstCharSearchUserEntity : SearchUserEntity, IFirstChars
{
    public string FirstChars { get; init; }
}

public class FirstCharEntity : WithUserIdEntity, IFirstChars, IEntityType
{
    public string FirstChars { get; init; }
    public FirstCharsEntityType EntityType { get; init; }
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
    public string PartitionKey { get; init; } = partitionKey;
    public string FirstChars { get; init; } = firstChars;
}

public class PartitionKeyAndIdAndFirstCharFilter(string partitionKey, string userId, string firstChars) :
    IMultipleParamFilter,
    IPartitionKey,
    IUserId,
    IFirstChars
{
    public string PartitionKey { get; init; } = partitionKey;
    public string UserId { get; init; } = userId;
    public string FirstChars { get; init; } = firstChars;
}
