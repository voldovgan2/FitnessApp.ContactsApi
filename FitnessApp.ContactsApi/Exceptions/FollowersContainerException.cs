using System;

namespace FitnessApp.ContactsApi.Exceptions;

public class FollowersContainerException(string message) : Exception(message);
