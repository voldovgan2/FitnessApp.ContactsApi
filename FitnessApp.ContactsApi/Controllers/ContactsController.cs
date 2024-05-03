using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FitnessApp.ContactsApi.Contracts.Input;
using FitnessApp.ContactsApi.Contracts.Output;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Services.Contacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.ContactsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    [Authorize]
    public class ContactsController : Controller
    {
        private readonly IContactsService _contactsService;
        private readonly IMapper _mapper;

        public ContactsController(
            IContactsService contactsService,
            IMapper mapper
        )
        {
            _contactsService = contactsService;
            _mapper = mapper;
        }

        [HttpPost("CreateUserContacts")]
        public async Task<UserContactsContract> CreateUserContacts([FromBody] CreateUserContactsContract contract)
        {
            var model = _mapper.Map<CreateUserContactsCollectionModel>(contract);
            var response = await _contactsService.CreateItemContacts(model);
            return _mapper.Map<UserContactsContract>(response);
        }

        [HttpGet("GetUserContacts")]
        public async Task<IEnumerable<UserContactsContract>> GetUserContacts([FromQuery]GetUserContactsContract contract)
        {
            var model = _mapper.Map<GetUserContactsModel>(contract);
            var response = await _contactsService.GetUserContacts(model);
            return response.Select(_mapper.Map<UserContactsContract>);
        }

        [HttpGet("GetIsFollower")]
        public async Task<bool> GetIsFollower([FromQuery]GetFollowerStatusContract contract)
        {
            var model = _mapper.Map<GetFollowerStatusModel>(contract);
            var response = await _contactsService.GetIsFollower(model);
            return response;
        }

        [HttpGet("GetUserContactsCount/{userId}")]
        public async Task<UserContactsCountContract> GetUserContactsCount([FromRoute]string userId)
        {
            var getUserFollowersModel = new GetUserContactsModel
            {
                UserId = userId,
                ContactsType = Enums.ContactsType.Followers
            };
            var userFollowers = _contactsService.GetUserContacts(getUserFollowersModel);
            var getUserFollowingsModel = new GetUserContactsModel
            {
                UserId = userId,
                ContactsType = Enums.ContactsType.Followings
            };
            var userFollowings = _contactsService.GetUserContacts(getUserFollowingsModel);
            await Task.WhenAll(userFollowers, userFollowings);
            return new UserContactsCountContract
            {
                UserId = userId,
                FollowersCount = userFollowers.Result.Count(),
                FollowingsCount = userFollowings.Result.Count(),
            };
        }

        [HttpPost("StartFollow")]
        public async Task<string> StartFollow([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var response = await _contactsService.StartFollow(model);
            return response;
        }

        [HttpPost("AcceptFollowRequest")]
        public async Task<string> AcceptFollowRequest([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var response = await _contactsService.AcceptFollowRequest(model);
            return response;
        }

        [HttpPost("RejectFollowRequest")]
        public async Task<string> RejectFollowRequest([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var response = await _contactsService.RejectFollowRequest(model);
            return response;
        }

        [HttpPost("DeleteFollowRequest")]
        public async Task<string> DeleteFollowRequest([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var response = await _contactsService.DeleteFollowRequest(model);
            return response;
        }

        [HttpPost("DeleteFollower")]
        public async Task<string> DeleteFollower([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var response = await _contactsService.DeleteFollower(model);
            return response;
        }

        [HttpPost("UnfollowUser")]
        public async Task<string> UnfollowUser([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var response = await _contactsService.UnfollowUser(model);
            return response;
        }
    }
}