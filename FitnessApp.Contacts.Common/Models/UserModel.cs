using FitnessApp.Common.Abstractions.Models;

namespace FitnessApp.Contacts.Common.Models;

public class UserModel : ICreateGenericModel
{
    required public string UserId { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
}
