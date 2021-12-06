using FitnessApp.Abstractions.Models.Collection;
using System.Collections.Generic;

namespace FitnessApp.ContactsApi.Models.Output
{
    public class UserContactsModel : ICollectionModel
    {
        public string UserId { get; set; }
        public Dictionary<string, IEnumerable<ICollectionItemModel>> Collection { get; set; }
    }
}
