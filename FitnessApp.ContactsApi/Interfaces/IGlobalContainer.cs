﻿using System.Threading.Tasks;
using FitnessApp.Common.Paged.Models.Output;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Models;

namespace FitnessApp.ContactsApi.Interfaces;

public interface IGlobalContainer
{
    Task AddUser(UserEntity userToAdd);
    Task UpdateUser(UserEntity userToUpdate);
    Task RemoveUser(UserEntity userToRemove);
    Task UpdateUser(UserEntity oldUser1, UserEntity newUser1);
    Task<PagedDataModel<SearchUserEntity>> GetUsers(GetUsersModel model);
}