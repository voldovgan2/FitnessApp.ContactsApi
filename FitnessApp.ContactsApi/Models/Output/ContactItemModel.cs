using FitnessApp.Abstractions.Models.Collection;

namespace FitnessApp.ContactsApi.Models.Output
{
    public class ContactItemModel : ISearchableCollectionItemModel
    {
        public string Id { get; set; }

        public bool Matches(string search)
        {           
            return true;
        }
    }
}
