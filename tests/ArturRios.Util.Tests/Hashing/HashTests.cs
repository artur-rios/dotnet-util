using ArturRios.Util.Hashing;

namespace ArturRios.Util.Tests.Hashing;

[Trait("Category", "Unit")]
public class HashTests
{
    private const string Text = "HelloWorld";

    // Cheap parameters keep the suite fast; the code path is identical to the production defaults, which cost
    // about 600 MB and a noticeable fraction of a second per hash. Only the test that proves the defaults are
    // applied pays that price.
    private static HashConfiguration CheapConfiguration =>
        new(degreeOfParallelism: 1, numberOfIterations: 1, memoryToUseInKb: 1024);

    [Fact]
    public void GivenTextSalt_WhenTextMatches_ThenMatchHash()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        Assert.True(Hash.TextMatches(Text, hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenSameTextAndSalt_WhenEncode_ThenProduceSameHash()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        Assert.Equal(hash, Hash.EncodeWithSalt(Text, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenSameText_WhenSaltsAreDifferent_ThenProduceDifferentHashes()
    {
        var hash1 = Hash.EncodeWithRandomSalt(Text, out var salt1, CheapConfiguration);
        var hash2 = Hash.EncodeWithRandomSalt(Text, out var salt2, CheapConfiguration);

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void GivenDifferentTextSalt_WhenTextMatches_ThenNotMatchHash()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        Assert.False(Hash.TextMatches("GoodbyeWorld", hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenTheRightTextAndADifferentSalt_WhenTextMatches_ThenNotMatchHash()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        var otherSalt = (byte[])salt.Clone();
        otherSalt[0] ^= 0xFF;

        Assert.False(Hash.TextMatches(Text, hash, otherSalt, CheapConfiguration));
    }

    [Fact]
    public void GivenNoConfiguration_WhenEncodeAndVerify_ThenUseTheDefaultCostParameters()
    {
        // The only test that runs Argon2 at the production defaults. A null configuration, in every method that
        // takes one, must be the default configuration.
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt);

        Assert.True(Hash.TextMatches(Text, hash, salt));
        Assert.Equal(Hash.EncodeWithSalt(Text, salt, new HashConfiguration()), hash);
        Assert.False(Hash.TextMatches(Text, hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenAnyText_WhenEncode_ThenProduceA128ByteHash()
    {
        Assert.Equal(128, Hash.EncodeWithSalt(Text, new byte[16], CheapConfiguration).Length);
        Assert.Equal(128, Hash.EncodeWithRandomSalt("x", out _, CheapConfiguration).Length);
    }

    [Fact]
    public void GivenTextsDifferingOnlyByCase_WhenEncodeWithTheSameSalt_ThenProduceDifferentHashes()
    {
        var salt = new byte[16];

        Assert.NotEqual(
            Hash.EncodeWithSalt("helloworld", salt, CheapConfiguration),
            Hash.EncodeWithSalt(Text, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenCustomConfiguration_WhenTextMatchesWithDifferentConfiguration_ThenNotMatch()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        var otherConfiguration = new HashConfiguration(degreeOfParallelism: 1, numberOfIterations: 2, memoryToUseInKb: 1024);

        // Before TextMatches accepted a configuration, this was the only reachable outcome for any hash
        // produced with non-default cost parameters.
        Assert.False(Hash.TextMatches(Text, hash, salt, otherConfiguration));
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

        var fromVerification = Assert.Throws<ArgumentException>(() =>
            Hash.TextMatches(string.Empty, new byte[128], new byte[16], CheapConfiguration));

        Assert.Equal("text", fromRandomSalt.ParamName);
        Assert.Equal("text", fromProvidedSalt.ParamName);
        Assert.Equal("text", fromVerification.ParamName);
    }

    [Fact]
    public void GivenNonAsciiText_WhenEncodeAndVerify_ThenRoundTrip()
    {
        const string text = "senha-ção-你好";

        var hash = Hash.EncodeWithRandomSalt(text, out var salt, CheapConfiguration);

        Assert.True(Hash.TextMatches(text, hash, salt, CheapConfiguration));
    }

    [Fact]
    public void GivenRandomSalt_WhenEncodeWithRandomSalt_ThenSaltIsSixteenBytes()
    {
        Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        Assert.Equal(16, salt.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    public void GivenTooShortSalt_WhenEncodeWithSalt_ThenThrowArgumentException(int saltLength)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Hash.EncodeWithSalt(Text, new byte[saltLength], CheapConfiguration));

        Assert.Equal("salt", exception.ParamName);
    }

    [Fact]
    public void GivenTooShortSalt_WhenTextMatches_ThenThrowArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Hash.TextMatches(Text, new byte[128], new byte[7], CheapConfiguration));

        Assert.Equal("salt", exception.ParamName);
    }

    [Fact]
    public void GivenEightByteSalt_WhenEncodeWithSalt_ThenSucceed()
    {
        var hash = Hash.EncodeWithSalt(Text, new byte[8], CheapConfiguration);

        Assert.NotEmpty(hash);
    }

    [Fact]
    public void GivenNullArguments_WhenEncodeOrVerify_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithSalt(null!, new byte[16], CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithSalt("text", null!, CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.EncodeWithRandomSalt(null!, out _, CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.TextMatches(null!, new byte[128], new byte[16], CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.TextMatches("text", null!, new byte[16], CheapConfiguration));
        Assert.Throws<ArgumentNullException>(() => Hash.TextMatches("text", new byte[128], null!, CheapConfiguration));
    }

    [Fact]
    public void GivenHashOfADifferentLength_WhenTextMatches_ThenReturnFalseInsteadOfThrowing()
    {
        var hash = Hash.EncodeWithRandomSalt(Text, out var salt, CheapConfiguration);

        Assert.False(Hash.TextMatches(Text, hash[..64], salt, CheapConfiguration));
        Assert.False(Hash.TextMatches(Text, [], salt, CheapConfiguration));
    }
}
