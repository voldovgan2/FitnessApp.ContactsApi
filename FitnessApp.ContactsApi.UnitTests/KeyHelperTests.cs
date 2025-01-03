using FitnessApp.Contacts.Common.Data;
using FitnessApp.Contacts.Common.Helpers;

namespace FitnessApp.ContactsApi.UnitTests;

public class KeyHelperTests
{
    [Theory]
    [InlineData("userId", "chars", "suffix", "userId_suffix_chars")]
    [InlineData("", "chars", "suffix", "suffix_chars")]
    [InlineData(null, "chars", "suffix", "suffix_chars")]
    [InlineData("userId", "", "suffix", "userId_suffix")]
    [InlineData("userId", null, "suffix", "userId_suffix")]
    [InlineData("userId", "chars", "", "userId_chars")]
    [InlineData("userId", "chars", null, "userId_chars")]
    [InlineData("", "", "suffix", "suffix")]
    [InlineData(null, null, "suffix", "suffix")]
    [InlineData(null, "", "suffix", "suffix")]
    [InlineData("", null, "suffix", "suffix")]
    [InlineData("", "chars", "", "chars")]
    [InlineData(null, "chars", null, "chars")]
    [InlineData(null, "chars", "", "chars")]
    [InlineData("", "chars", null, "chars")]
    [InlineData("userId", "", "", "userId")]
    [InlineData("userId", null, null, "userId")]
    [InlineData("userId", "", null, "userId")]
    [InlineData("userId", null, "", "userId")]
    [InlineData(null, null, null, "")]
    [InlineData(null, null, "", "")]
    [InlineData(null, "", null, "")]
    [InlineData(null, "", "", "")]
    [InlineData("", null, null, "")]
    [InlineData("", null, "", "")]
    [InlineData("", "", null, "")]
    [InlineData("", "", "", "")]
    public void CreateKeyByChars_ReturnsExpected(string userId, string chars, string suffix, string expected)
    {
        var result = KeyHelper.CreateKeyByChars(userId, chars, suffix);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetKeysByFirstChars_ReturnsExpected()
    {
        var result = KeyHelper.GetKeysByFirstChars(
            new FirstCharSearchUserEntity
            {
                FirstName = "abcdef",
                LastName = "fedcba",
            },
            1,
            4);
        Assert.Contains(result, o => o == "ab");
        Assert.Contains(result, o => o == "abc");
        Assert.Contains(result, o => o == "abcd");
        Assert.Contains(result, o => o == "fe");
        Assert.Contains(result, o => o == "fed");
        Assert.Contains(result, o => o == "fedc");
    }

    [Fact]
    public void GetUnMatchedKeys_ReturnsExpected()
    {
        var (KeysToRemove, KeysToAdd) = KeyHelper.GetUnMatchedKeys(
            new FirstCharSearchUserEntity
            {
                FirstName = "abcdef",
                LastName = "cdefgh",
            },
            new FirstCharSearchUserEntity
            {
                FirstName = "abdefg",
                LastName = "cefghij",
            },
            4);
        Assert.Contains(KeysToRemove, o => o == "abc");
        Assert.Contains(KeysToRemove, o => o == "abcd");
        Assert.Contains(KeysToRemove, o => o == "cd");
        Assert.Contains(KeysToRemove, o => o == "cde");
        Assert.Contains(KeysToRemove, o => o == "cdef");
        Assert.Contains(KeysToAdd, o => o == "abd");
        Assert.Contains(KeysToAdd, o => o == "abde");
        Assert.Contains(KeysToAdd, o => o == "ce");
        Assert.Contains(KeysToAdd, o => o == "cef");
        Assert.Contains(KeysToAdd, o => o == "cefg");
    }
}