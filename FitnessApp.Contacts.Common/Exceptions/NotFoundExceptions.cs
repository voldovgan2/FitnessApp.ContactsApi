using FitnessApp.Contacts.Common.Data;

namespace FitnessApp.Contacts.Common.Exceptions;

public class FollowerNotFoundException(string userId, string followerId) :
    Exception($"Follower with userId: {userId} and followerId: {followerId} not exist");

public class FollowingNotFoundException(string userId, string followingId) :
    Exception($"Following with userId: {userId} and followingId: {followingId} not exist");

public class FollowerRequestNotFoundException(string thisId, string otherId) :
    Exception($"Following request with thisId: {thisId} and otherId: {otherId} not exist");

public class FirstCharSearchUserNotFoundException(string partitionKey, string userId, string firstChars) :
    Exception($"FirstCharSearchUser with partitionKey: {partitionKey} and userId: {userId} and firstChars: {firstChars} not exist");

public class FirstCharEntityNotFoundException(string userId, string firstChars, FirstCharsEntityType firstCharsEntityType) :
    Exception($"FirstCharSearchUser with userId: {userId} and firstChars: {firstChars} and firstCharsEntityType: {firstCharsEntityType} not exist");
