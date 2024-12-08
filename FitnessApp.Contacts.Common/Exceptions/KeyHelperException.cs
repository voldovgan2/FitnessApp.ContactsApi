using System;

namespace FitnessApp.Contacts.Common.Exceptions;

public class KeyHelperException(string error) : Exception(error);
