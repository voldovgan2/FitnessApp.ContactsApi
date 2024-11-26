using System;
using System.Collections.Generic;
using FitnessApp.ContactsApi.Data;
using FitnessApp.ContactsApi.Exceptions;

namespace FitnessApp.ContactsApi.Helpers;

public static class CategoryHelper
{
    private const int _followersCoefitient = 1000;
    private const int _dateCoefitient = 0;
    private const int _threeshold = 10;
    private const int _threesholdDays = 7;
    private const int _k10 = 10000 / _followersCoefitient;
    private const int _k50 = 50000 / _followersCoefitient;
    private const int _k500 = 500000 / _followersCoefitient;
    private const int _m10 = 10000000 / _followersCoefitient;
    private const int _m100 = 100000000 / _followersCoefitient;
    private static readonly Dictionary<byte, int> _topThreesholds = new()
    {
        { 1, _k10 },
        { 2, _k50 },
        { 3, _k500 },
        { 4, _m10 },
        { 5, _m100 },
    };
    private static readonly Dictionary<byte, int> _bottomThreesholds = new()
    {
        { 1, _k10 - (_k10 * _threeshold / 100) },
        { 2, _k50 - (_k50 * _threeshold / 100) },
        { 3, _k500 - (_k500 * _threeshold / 100) },
        { 4, _m10 - (_m10 * _threeshold / 100) },
        { 5, _m100 - (_m100 * _threeshold / 100) },
    };
    private static readonly Dictionary<byte, byte> _upgradeCategory = new()
    {
        { 1, 2 },
        { 2, 3 },
        { 3, 4 },
        { 4, 5 },
        { 5, 5 },
    };
    private static readonly Dictionary<byte, byte> _downgradeCategory = new()
    {
        { 1, 1 },
        { 2, 1 },
        { 3, 2 },
        { 4, 3 },
        { 5, 4 },
    };
    private static readonly Dictionary<byte, byte> _charsCountCategory = new()
    {
        { 1, 1 },
        { 2, 2 },
        { 3, 3 },
        { 4, 4 },
        { 5, 5 },
    };

    public static bool ShouldDowngradeCategory(UserEntity user, DateTime now)
    {
        if (user.Category == 1)
            return false;
        var threeshold = GetItemFomCollection(_bottomThreesholds, user.Category);
        return user.FollowersCount < threeshold && IsOutsideDate(user, now);
    }

    public static bool ShouldUpgradeCategory(UserEntity user, DateTime now)
    {
        if (user.Category == 5)
            return false;
        var threeshold = GetItemFomCollection(_topThreesholds, user.Category);
        return user.FollowersCount > threeshold;
    }

    public static byte GetUpgradeCategory(byte category)
    {
        return GetItemFomCollection(_upgradeCategory, category);
    }

    public static byte GetDowngradeCategory(byte category)
    {
        return GetItemFomCollection(_downgradeCategory, category);
    }

    public static byte GetCategoryCharsCount(byte category)
    {
        return GetItemFomCollection(_charsCountCategory, category);
    }

    private static T GetItemFomCollection<T>(Dictionary<byte, T> collection, byte category)
    {
        if (!collection.TryGetValue(category, out var result))
            throw new CategoryServiceException($"Invalid category: {category}");
        return result;
    }

    private static bool IsOutsideDate(UserEntity user, DateTime now)
    {
        return user.CategoryDate < now.AddDays(-_threesholdDays * _dateCoefitient);
    }
}
