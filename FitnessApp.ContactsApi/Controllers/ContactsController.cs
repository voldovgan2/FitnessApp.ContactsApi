﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.Common.Abstractions.Controllers;
using FitnessApp.ContactsApi.Contracts.Input;
using FitnessApp.ContactsApi.Contracts.Output;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Services.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.ContactsApi.Controllers;

public class ContactsController(IContactsService contactsService, IMapper mapper) : FitnessAppBaseController
{
    [HttpGet("GetUserContacts")]
    public async Task<IEnumerable<UserContactsContract>> GetUserContacts([FromQuery]GetUserContactsContract contract)
    {
        var model = mapper.Map<GetUserContactsModel>(contract);
        var response = await contactsService.GetUserContacts(model);
        return response.Select(mapper.Map<UserContactsContract>);
    }

    [HttpGet("GetIsFollower")]
    public async Task<bool> GetIsFollower([FromQuery]GetFollowerStatusContract contract)
    {
        var model = mapper.Map<GetFollowerStatusModel>(contract);
        var response = await contactsService.GetIsFollower(model);
        return response;
    }

    [HttpPost("GetIsFollowers")]
    public async Task<IEnumerable<FollowerStatusContract>> GetIsFollowers([FromBody] GetFollowersStatusContract contract)
    {
        var model = mapper.Map<GetFollowersStatusModel>(contract);
        var response = await contactsService.GetIsFollowers(model);
        return response.Select(mapper.Map<FollowerStatusContract>);
    }

    [HttpGet("GetUserContactsCount/{userId}")]
    public async Task<UserContactsCountContract> GetUserContactsCount([FromRoute]string userId)
    {
        var getUserFollowersModel = new GetUserContactsModel
        {
            UserId = userId,
            ContactsType = Enums.ContactsType.Followers
        };
        var userFollowers = contactsService.GetUserContacts(getUserFollowersModel);
        var getUserFollowingsModel = new GetUserContactsModel
        {
            UserId = userId,
            ContactsType = Enums.ContactsType.Followings
        };
        var userFollowings = contactsService.GetUserContacts(getUserFollowingsModel);
        await Task.WhenAll(userFollowers, userFollowings);
        return new UserContactsCountContract
        {
            UserId = userId,
            FollowersCount = userFollowers.Result.Count(),
            FollowingsCount = userFollowings.Result.Count(),
        };
    }

    [HttpPost("CreateUserContacts")]
    public async Task<UserContactsContract> CreateUserContacts([FromBody] CreateUserContactsContract contract)
    {
        var model = mapper.Map<CreateUserContactsCollectionModel>(contract);
        var response = await contactsService.CreateItemContacts(model);
        return mapper.Map<UserContactsContract>(response);
    }

    [HttpPost("StartFollow")]
    public async Task<string> StartFollow([FromBody] SendFollowContract contract)
    {
        var model = mapper.Map<SendFollowModel>(contract);
        var response = await contactsService.StartFollow(model);
        return response;
    }

    [HttpPost("AcceptFollowRequest")]
    public async Task<string> AcceptFollowRequest([FromBody] ProcessFollowRequestContract contract)
    {
        var model = mapper.Map<ProcessFollowRequestModel>(contract);
        var response = await contactsService.AcceptFollowRequest(model);
        return response;
    }

    [HttpPost("RejectFollowRequest")]
    public async Task<string> RejectFollowRequest([FromBody] ProcessFollowRequestContract contract)
    {
        var model = mapper.Map<ProcessFollowRequestModel>(contract);
        var response = await contactsService.RejectFollowRequest(model);
        return response;
    }

    [HttpPost("DeleteFollowRequest")]
    public async Task<string> DeleteFollowRequest([FromBody] SendFollowContract contract)
    {
        var model = mapper.Map<SendFollowModel>(contract);
        var response = await contactsService.DeleteFollowRequest(model);
        return response;
    }

    [HttpPost("DeleteFollower")]
    public async Task<string> DeleteFollower([FromBody] ProcessFollowRequestContract contract)
    {
        var model = mapper.Map<ProcessFollowRequestModel>(contract);
        var response = await contactsService.DeleteFollower(model);
        return response;
    }

    [HttpPost("UnfollowUser")]
    public async Task<string> UnfollowUser([FromBody] SendFollowContract contract)
    {
        var model = mapper.Map<SendFollowModel>(contract);
        var response = await contactsService.UnfollowUser(model);
        return response;
    }
}