using System.Collections.Generic;
using FitnessApp.Common.Abstractions.Models.Collection;

namespace FitnessApp.ContactsApi.Models.Output;

public class UserContactsCollectionModel : ICollectionModel
{
    public string UserId { get; set; }
    public Dictionary<string, List<ICollectionItemModel>> Collection { get; set; }
}
