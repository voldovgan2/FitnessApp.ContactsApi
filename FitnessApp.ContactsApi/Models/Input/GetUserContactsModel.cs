using FitnessApp.ContactsApi.Enums;

namespace FitnessApp.ContactsApi.Models.Input;

public class GetUserContactsModel
{
    public string UserId { get; set; }
    public ContactsType ContactsType { get; set; }
}