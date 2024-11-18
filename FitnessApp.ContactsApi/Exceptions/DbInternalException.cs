using System;
namespace FitnessApp.ContactsApi.Exceptions;

public class DbInternalException(string error) : Exception(error);
