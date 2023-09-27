using System.Linq;
using System.Net;
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

    // [Authorize]
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
        public async Task<IActionResult> CreateUserContacts([FromBody] CreateUserContactsContract contract)
        {
            var model = _mapper.Map<CreateUserContactsCollectionModel>(contract);
            var created = await _contactsService.CreateItemContacts(model);
            if (created != null)
            {
                var result = _mapper.Map<UserContactsContract>(created);
                return Ok(result);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("GetUserContacts")]
        public async Task<IActionResult> GetUserContacts([FromQuery]GetUserContactsContract contract)
        {
            var model = _mapper.Map<GetUserContactsModel>(contract);
            var result = await _contactsService.GetUserContacts(model);
            if (result != null)
            {
                return Ok(result.Select(i => _mapper.Map<UserContactsContract>(i)));
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("GetIsFollower")]
        public async Task<IActionResult> GetIsFollower([FromQuery]GetFollowerStatusContract contract)
        {
            var model = _mapper.Map<GetFollowerStatusModel>(contract);
            var result = await _contactsService.GetIsFollower(model);
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
        public async Task<IActionResult> GetUserContactsCount([FromRoute]string userId)
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
        public async Task<IActionResult> StartFollow([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var updated = await _contactsService.StartFollow(model);
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
        public async Task<IActionResult> AcceptFollowRequest([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.AcceptFollowRequest(model);
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
        public async Task<IActionResult> RejectFollowRequest([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.RejectFollowRequest(model);
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
        public async Task<IActionResult> DeleteFollowRequest([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var updated = await _contactsService.DeleteFollowRequest(model);
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
        public async Task<IActionResult> DeleteFollower([FromBody] ProcessFollowRequestContract contract)
        {
            var model = _mapper.Map<ProcessFollowRequestModel>(contract);
            var updated = await _contactsService.DeleteFollower(model);
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
        public async Task<IActionResult> UnfollowUser([FromBody] SendFollowContract contract)
        {
            var model = _mapper.Map<SendFollowModel>(contract);
            var updated = await _contactsService.UnfollowUser(model);
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