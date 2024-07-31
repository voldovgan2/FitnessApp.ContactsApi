using System.Collections.Generic;

namespace FitnessApp.ContactsApi.Contracts.Input;

public class GetFollowersStatusContract
{
    public IEnumerable<string> UserIds { get; set; }
    public string ContactsUserId { get; set; }
}
