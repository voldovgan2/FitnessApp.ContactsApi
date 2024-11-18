using System;
using System.Collections.Generic;
using System.Linq;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Exceptions;

namespace FitnessApp.ContactsApi.Helpers;

public static class KeyHelper
{
    public static string CreateKeyByChars(string chars)
    {
        return $"{chars}";
    }

    public static string CreateKeyByChars(string userId, string chars, string suffix = null)
    {
        string[] values = [userId, suffix, chars];
        return string.Join("_", values.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    public static HashSet<string> GetKeysByFirstChars(UserEntityBase user, int left, int count)
    {
        if (left < 0 || count < 0)
            throw new KeyHelperException($"Invalid range: left: {left}, count: {count}");
        var result = new HashSet<string>();
        for (int k = 1; k <= count; k++)
        {
            result.Add(GetSubstring(user.FirstName, left, k));
            result.Add(GetSubstring(user.LastName, left, k));
        }

        return result;
    }

    public static string GetSubstring(string value, int startIndex, int count)
    {
        return value
            .Substring(startIndex, Math.Min(value.Length, count))
            .ToLower();
    }

    public static (HashSet<string> KeysToRemove, HashSet<string> KeysToAdd) GetUnMatchedKeys(UserEntityBase user1, UserEntityBase user2, int charsCount)
    {
        var oldKeys = GetKeysByFirstChars(user1, 0, charsCount);
        var newKeys = GetKeysByFirstChars(user2, 0, charsCount);
        var keysToRemove = oldKeys.Except(newKeys).ToHashSet();
        var keyToAdd = newKeys.Except(oldKeys).ToHashSet();
        return (keysToRemove, keyToAdd);
    }
}
