using System;
namespace FitnessApp.Contacts.Common.Exceptions;

public class DbInternalException(string error) : Exception(error);
