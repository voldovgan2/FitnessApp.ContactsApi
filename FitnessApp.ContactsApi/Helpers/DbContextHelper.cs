using System;
using FitnessApp.Common.Abstractions.Db;
using FitnessApp.ContactsApi.Data;
using MongoDB.Driver;

namespace FitnessApp.ContactsApi.Helpers;

public static class DbContextHelper
{
    public static FilterDefinition<TEntity> CreateGetByUserIdFiter<TEntity>(string userId)
        where TEntity : IWithUserIdEntity
    {
        return Builders<TEntity>.Filter.Eq(s => s.UserId, userId);
    }

    public static FilterDefinition<TFollower> CreateGetByUserIdAndFollowerIdFiter<TFollower>(string userId, string followerId)
        where TFollower : IFollowerEntity
    {
        return Builders<TFollower>
                        .Filter
                        .And(
                            CreateGetByUserIdFiter<TFollower>(userId),
                            Builders<TFollower>
                                .Filter
                                .Eq(s => s.FollowerId, followerId)
                                );
    }

    public static FilterDefinition<TFollowing> CreateGetByUserIdAndFollowingIdFiter<TFollowing>(string userId, string followingId)
        where TFollowing : IFollowingEntity
    {
        return Builders<TFollowing>
                        .Filter
                        .And(
                            CreateGetByUserIdFiter<TFollowing>(userId),
                            Builders<TFollowing>
                                .Filter
                                .Eq(s => s.FollowingId, followingId)
                                );
    }

    public static FilterDefinition<TFirstChars> CreateGetByFirstCharsFiter<TFirstChars>(string firstChars)
        where TFirstChars : IFirstChars
    {
        return Builders<TFirstChars>.Filter.Eq(s => s.FirstChars, firstChars);
    }

    public static FilterDefinition<TEntityType> CreateGetByEntityTypeFiter<TEntityType>(FirstCharsEntityType entityType)
        where TEntityType : IEntityType
    {
        return Builders<TEntityType>.Filter.Eq(s => s.EntityType, entityType);
    }
}
