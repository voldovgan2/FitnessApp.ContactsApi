﻿using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IContactsService
{
    Task<PagedDataModel<UserModel>> GetUsers(GetUsersModel model);
    Task<PagedDataModel<UserModel>> GetUserFollowers(string userId, GetUsersModel model);
    Task AddUser(UserModel user);
    Task FollowUser(string userId, string userToFollowId);
    Task UnFollowUser(string userId, string userToUnFollowId);
    Task UpdateUser(UserEntity oldUser, UserEntity newUser);
}
