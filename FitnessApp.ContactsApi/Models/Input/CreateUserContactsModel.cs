using FitnessApp.Abstractions.Models.Collection;
using System.Collections.Generic;

namespace FitnessApp.ContactsApi.Models.Input
{
    public class CreateUserContactsModel : ICreateCollectionModel
    {
        public string UserId { get; set; }
        public Dictionary<string, IEnumerable<ICollectionItemModel>> Collection { get; set; }
    }
}
