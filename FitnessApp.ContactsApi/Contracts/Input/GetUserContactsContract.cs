using FitnessApp.ContactsApi.Enums;

namespace FitnessApp.ContactsApi.Contracts.Input;

public class GetUserContactsContract
{
    public string UserId { get; set; }
    public ContactsType ContactsType { get; set; }
}