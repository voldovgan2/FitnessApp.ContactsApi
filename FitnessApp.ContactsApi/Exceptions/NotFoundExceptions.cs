using System;
using FitnessApp.ContactsApi.Data;

namespace FitnessApp.ContactsApi.Exceptions;

public class EntityNotFoundException(string entity, string id) :
    Exception($"Entity {entity} with id: {id} not exist");

public class FollowerNotFoundException(string id, string followerId) :
    Exception($"Follower with id: {id} and followerId: {followerId} not exist");

public class FollowingNotFoundException(string id, string followingId) :
    Exception($"Following with id: {id} and followingId: {followingId} not exist");

public class FollowerRequestNotFoundException(string thisId, string otherId) :
    Exception($"Following request with thisId: {thisId} and otherId: {otherId} not exist");

public class FirstCharSearchUserNotFoundException(string partitionKey, string id, string firstChars) :
    Exception($"FirstCharSearchUser with partitionKey: {partitionKey} and id: {id} and firstChars: {firstChars} not exist");

public class FirstCharEntityNotFoundException(string id, string firstChars, FirstCharsEntityType firstCharsEntityType) :
    Exception($"FirstCharSearchUser with id: {id} and firstChars: {firstChars} and firstCharsEntityType: {firstCharsEntityType} not exist");
