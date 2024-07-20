using System.Collections.Generic;
using FitnessApp.Common.Abstractions.Models.Collection;

namespace FitnessApp.ContactsApi.Models.Input;

public class CreateUserContactsCollectionModel : ICreateCollectionModel
{
    public string UserId { get; set; }
    public Dictionary<string, IEnumerable<ICollectionItemModel>> Collection { get; set; }
}
