using System;

namespace FitnessApp.ContactsApi.Exceptions;

public class KeyHelperException(string error) : Exception(error);
