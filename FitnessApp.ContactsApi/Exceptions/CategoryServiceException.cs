using System;

namespace FitnessApp.ContactsApi.Exceptions;

public class CategoryServiceException(string message) : Exception(message);
