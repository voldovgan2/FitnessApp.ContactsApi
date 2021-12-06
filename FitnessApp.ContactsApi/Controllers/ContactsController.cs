using FitnessApp.ContactsApi.Contracts.Input;
using FitnessApp.ContactsApi.Contracts.Output;
using FitnessApp.ContactsApi.Data.Entities;
using FitnessApp.ContactsApi.Models.Input;
using FitnessApp.ContactsApi.Models.Output;
using FitnessApp.ContactsApi.Services.Contacts;
using FitnessApp.Serializer.JsonMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FitnessApp.ContactsApi.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel> _contactsService;
        private readonly IJsonMapper _mapper;

        public ContactsController
        (
            IContactsService<UserContactsEntity, ContactItemEntity, UserContactsModel, ContactItemModel, CreateUserContactsModel, UpdateUserContactModel> contactsService,
            IJsonMapper mapper
        )
        {
            _contactsService = contactsService;
            _mapper = mapper;
        }

        [HttpGet("GetUserContacts")]
        public async Task<IActionResult> GetUserContactsAsync([FromQuery]GetUserContactsContract contract)
        {
            var model = _mapper.Convert<GetUserContactsModel>(contract);
            var result = await _contactsService.GetUserContacts(model);
            if (result != null)
            {
                return Ok(result.Select(i => new ContactContract { UserId = i.Id }));
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("GetIsFollower")]
        public async Task<IActionResult> GetIsFollowerAsync([FromQuery]GetFollowerStatusContract contract)
        {
            var model = _mapper.Convert<GetFollowerStatusModel>(contract);
            var result = await _contactsService.GetIsFollowerAsync(model);
            if (result != null)
            {
                return Ok(result.Value);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("GetUserContactsCount/{userId}")]
        public async Task<IActionResult> GetUserContactsCountAsync([FromRoute]string userId)
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
            if (userFollowers.Result != null && userFollowings.Result != null)
            {
                return Ok(new UserContactsCountContract 
                {
                    UserId = userId,
                    FollowersCount = userFollowers.Result.Count(),
                    FollowingsCount = userFollowings.Result.Count(),
                });
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("StartFollow")]
        public async Task<IActionResult> StartFollowAsync([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Convert<SendFollowModel>(contract);
            var updated = await _contactsService.StartFollowAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("AcceptFollowRequest")]
        public async Task<IActionResult> AcceptFollowRequestAsync([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Convert<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.AcceptFollowRequestAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("RejectFollowRequest")]
        public async Task<IActionResult> RejectFollowRequestAsync([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Convert<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.RejectFollowRequestAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("DeleteFollowRequest")]
        public async Task<IActionResult> DeleteFollowRequestAsync([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Convert<SendFollowModel>(contract);
            var updated = await _contactsService.DeleteFollowRequestAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("DeleteFollower")]
        public async Task<IActionResult> DeleteFollowerAsync([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Convert<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.DeleteFollowerAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("UnfollowUser")]
        public async Task<IActionResult> UnfollowUserAsync([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Convert<SendFollowModel>(contract);
            var updated = await _contactsService.UnfollowUserAsync(model);
            if (updated != null)
            {
                return Ok(updated);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }    
}