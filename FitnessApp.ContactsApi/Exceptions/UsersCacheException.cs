using System;

namespace FitnessApp.ContactsApi.Exceptions;

public class UsersCacheException(string message) : Exception(message);
