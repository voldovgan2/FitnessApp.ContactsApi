using System;

namespace FitnessApp.Contacts.Common.Exceptions;

public class CategoryServiceException(string message) : Exception(message);
