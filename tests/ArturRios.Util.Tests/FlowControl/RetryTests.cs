using ArturRios.Util.FlowControl;

namespace ArturRios.Util.Tests.FlowControl;

public class RetryTests
{
    private int _attemptCount;

    [Fact]
    public void Should_RetryWithResult()
    {
        const int maxAttempts = 3;
        const int delayMilliseconds = 1000;

        var result = Retry.New
            .MaxAttempts(maxAttempts)
            .DelayMilliseconds(delayMilliseconds)
            .Execute(() => TestMethod(maxAttempts));

        Assert.Equal(maxAttempts, result);
    }

    [Fact]
    public void Should_RetryVoidMethod()
    {
        const int maxAttempts = 3;
        const int delayMilliseconds = 1000;

        var retry = Retry.New
            .MaxAttempts(maxAttempts)
            .DelayMilliseconds(delayMilliseconds);

        var exception = Record.Exception(() => retry.Execute(() => VoidTestMethod(maxAttempts)));

        Assert.Null(exception);
    }

    private int TestMethod(int maxAttempts)
    {
        _attemptCount++;

        return _attemptCount < maxAttempts ? throw new Exception("Simulated failure") : _attemptCount;
    }

    private void VoidTestMethod(int maxAttempts)
    {
        _attemptCount++;

        if (_attemptCount < maxAttempts)
        {
            throw new Exception("Simulated failure");
        }
    }
}