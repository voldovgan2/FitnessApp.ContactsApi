using System.Collections.Generic;

namespace FitnessApp.ContactsApi.Models.Input;

public class GetFollowersStatusModel
{
    public IEnumerable<string> UserIds { get; set; }
    public string ContactsUserId { get; set; }
}
