using System.Text;
using App.Api.Shared.Extensions;
using Xunit;

namespace TestBase.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("something-url-friendly", "something-url-friendly")]
    [InlineData("sömething-nöt-ürl-friéndly", "something-not-url-friendly")]
    [InlineData("\not/okay right?", "otokay-right")]
    [InlineData("wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww", "wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww")]
    [InlineData("", "")]
    [InlineData("UPPER CASE", "upper-case")]
    public void Should_ReturnExpectedString_When_TransformingToUrlFriendly(
        string input, string expected)
    {
        Assert.Equal(expected, input.UrlFriendly());
    }

    [Fact]
    public void Should_ChangeGuids_When_TransformingToUrlFriendly()
    {
        var guid = Guid.NewGuid().ToString();
        Assert.NotEqual(guid, guid.UrlFriendly());
    }

    [Fact]
    public void Should_EncodeStringToBase64_When_InputIsValid()
    {
        var value = "some-random-string";
        var result = value.To64();

        Assert.NotEmpty(result);
        Assert.NotEqual(value, result);

        var bytes = Convert.FromBase64String(result);

        Assert.Equal(value, Encoding.ASCII.GetString(bytes));
    }

    [Fact]
    public void Should_DecodeBase64String_When_InputIsValid()
    {
        var value = "some-random-string";
        var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
        var result = encoded.From64();

        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public void Should_ReturnEmptyString_When_InputIsNotBase64()
    {
        var value = "some-random-string";
        var result = value.From64();

        Assert.Null(result);
    }
}