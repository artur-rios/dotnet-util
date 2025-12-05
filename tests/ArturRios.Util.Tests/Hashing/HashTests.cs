using ArturRios.Util.Hashing;

namespace ArturRios.Util.Tests.Hashing;

public class HashTests
{
    [Fact]
    public void Should_MatchHash()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt);
        var matches = Hash.TextMatches(text, hash, salt);

        Assert.True(matches);
    }

    [Fact]
    public void Should_ProduceSameHash_When_HashesAreFromSameTextAndSalt()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt);
        var testHash = Hash.EncodeWithSalt(text, salt);

        var matches = hash.SequenceEqual(testHash);

        Assert.True(matches);
    }

    [Fact]
    public void Should_ProduceDifferentHashes_When_TextSameButSaltIsDifferent()
    {
        const string text = "HelloWorld";

        var hash1 = Hash.EncodeWithRandomSalt(text, out var salt1);
        var hash2 = Hash.EncodeWithRandomSalt(text, out var salt2);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void ShouldNot_MatchHash()
    {
        const string text1 = "HelloWorld";
        const string text2 = "GoodbyeWorld";

        var hash = Hash.EncodeWithRandomSalt(text1, out var salt);

        var matches = Hash.TextMatches(text2, hash, salt);

        Assert.False(matches);
    }
}