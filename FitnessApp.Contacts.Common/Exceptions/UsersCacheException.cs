using System;

namespace FitnessApp.Contacts.Common.Exceptions;

public class UsersCacheException(string message) : Exception(message);
