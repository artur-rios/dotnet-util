using ArturRios.Util.Hashing;

namespace ArturRios.Util.Tests.Hashing;

public class HashTests
{
    [Fact]
    public void GivenTextSalt_WhenTextMatches_ThenMatchHash()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt);
        var matches = Hash.TextMatches(text, hash, salt);

        Assert.True(matches);
    }

    [Fact]
    public void GivenSameTextAndSalt_WhenEncode_ThenProduceSameHash()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt);
        var testHash = Hash.EncodeWithSalt(text, salt);

        var matches = hash.SequenceEqual(testHash);

        Assert.True(matches);
    }

    [Fact]
    public void GivenSameText_WhenSaltsAreDifferent_ThenProduceDifferentHashes()
    {
        const string text = "HelloWorld";

        var hash1 = Hash.EncodeWithRandomSalt(text, out var salt1);
        var hash2 = Hash.EncodeWithRandomSalt(text, out var salt2);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void GivenDifferentTextSalt_WhenTextMatches_ThenNotMatchHash()
    {
        const string text1 = "HelloWorld";
        const string text2 = "GoodbyeWorld";

        var hash = Hash.EncodeWithRandomSalt(text1, out var salt);

        var matches = Hash.TextMatches(text2, hash, salt);

        Assert.False(matches);
    }

    // Cheap parameters keep the suite fast; the code path is identical to the production defaults.
    private static HashConfiguration CheapConfiguration =>
        new(degreeOfParallelism: 1, numberOfIterations: 1, memoryToUseInKb: 1024);

    [Fact]
    public void GivenCustomConfiguration_WhenTextMatchesWithTheSameConfiguration_ThenMatch()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt, CheapConfiguration);

        Assert.True(Hash.TextMatches(text, hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenCustomConfiguration_WhenTextMatchesWithDifferentConfiguration_ThenNotMatch()
    {
        const string text = "HelloWorld";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt, CheapConfiguration);

        var otherConfiguration = new HashConfiguration(degreeOfParallelism: 1, numberOfIterations: 2, memoryToUseInKb: 1024);

        // Before TextMatches accepted a configuration, this was the only reachable outcome for any hash
        // produced with non-default cost parameters.
        Assert.False(Hash.TextMatches(text, hash, salt, otherConfiguration));
    }

    [Fact]
    public void GivenEmptyText_WhenEncode_ThenThrowArgumentExceptionNamingTheTextParameter()
    {
        // Argon2 rejects a zero-length password, but reports it against a "password" parameter this API
        // does not have. Catch it at the boundary instead.
        var fromRandomSalt = Assert.Throws<ArgumentException>(() =>
            Hash.EncodeWithRandomSalt(string.Empty, out _, CheapConfiguration));

        var fromProvidedSalt = Assert.Throws<ArgumentException>(() =>
            Hash.EncodeWithSalt(string.Empty, new byte[16], CheapConfiguration));

        Assert.Equal("text", fromRandomSalt.ParamName);
        Assert.Equal("text", fromProvidedSalt.ParamName);
    }

    [Fact]
    public void GivenNonAsciiText_WhenEncodeAndVerify_ThenRoundTrip()
    {
        const string text = "senha-\u00e7\u00e3o-\u4f60\u597d";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt, CheapConfiguration);

        Assert.True(Hash.TextMatches(text, hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenRandomSalt_WhenEncodeWithRandomSalt_ThenSaltIsSixteenBytes()
    {
        Hash.EncodeWithRandomSalt("HelloWorld", out var salt, CheapConfiguration);

        Assert.Equal(16, salt.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    public void GivenTooShortSalt_WhenEncodeWithSalt_ThenThrowArgumentException(int saltLength)
    {
        Assert.Throws<ArgumentException>(() => Hash.EncodeWithSalt("HelloWorld", new byte[saltLength], CheapConfiguration));
    }

    [Fact]
    public void GivenEightByteSalt_WhenEncodeWithSalt_ThenSucceed()
    {
        var hash = Hash.EncodeWithSalt("HelloWorld", new byte[8], CheapConfiguration);

        Assert.NotEmpty(hash);
    }

    [Fact]
    public void GivenNullArguments_WhenEncodeOrVerify_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithSalt(null!, new byte[16], CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithSalt("text", null!, CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithRandomSalt(null!, out _, CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.TextMatches("text", null!, new byte[16], CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.TextMatches("text", new byte[128], null!, CheapConfiguration));
    }

    [Fact]
    public void GivenHashOfADifferentLength_WhenTextMatches_ThenReturnFalseInsteadOfThrowing()
    {
        var hash = Hash.EncodeWithRandomSalt("HelloWorld", out var salt, CheapConfiguration);

        Assert.False(Hash.TextMatches("HelloWorld", hash[..64], salt, CheapConfiguration));
        Assert.False(Hash.TextMatches("HelloWorld", [], salt, CheapConfiguration));
    }
}
