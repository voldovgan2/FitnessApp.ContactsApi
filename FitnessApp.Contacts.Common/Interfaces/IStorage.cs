﻿using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Events;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.Contacts.Common.Interfaces;

public interface IStorage
{
    Task<UserEntity> GetUser(string userId);
    Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserEntity user);
    Task UpdateUser(UserEntity user);
    Task<bool> IsFollower(string userId, string userToFollowId);
    Task AddFollower(UserEntity user, string userToFollowId);
    Task RemoveFollower(UserEntity user, string userToUnFollowId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
    Task HandleCategoryChange(CategoryChangedEvent @event);
}
