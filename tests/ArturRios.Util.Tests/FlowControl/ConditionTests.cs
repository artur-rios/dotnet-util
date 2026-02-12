using ArturRios.Util.FlowControl;

namespace ArturRios.Util.Tests.FlowControl;

public class ConditionTests
{
    [Fact]
    public void GivenTrueCondition_WhenToProcessOutput_ThenSucceed()
    {
        var output = Condition.Create.True(true).FailsWith("Condition should be true").ToProcessOutput();

        Assert.True(output.Success);
    }

    [Fact]
    public void GivenMultipleTrueExpressions_WhenToProcessOutput_ThenSucceed()
    {
        var output = Condition.Create
            .True(true).FailsWith("Condition 1 should be true")
            .False(false).FailsWith("Condition 2 should be false")
            .ToProcessOutput();

        Assert.True(output.Success);
    }

    [Fact]
    public void GivenFalseCondition_WhenToProcessOutput_ThenNotSucceed()
    {
        var output = Condition.Create.True(false).FailsWith("Condition should be true").ToProcessOutput();

        Assert.False(output.Success);
    }

    [Fact]
    public void GivenMixedExpressions_WhenToProcessOutput_ThenNotSucceedAndReturnFirstError()
    {
        var output = Condition.Create
            .True(true).FailsWith("Condition 1 should be true")
            .False(false).FailsWith("Condition 2 should be false")
            .True(false).FailsWith("Condition 3 should be true")
            .ToProcessOutput();

        Assert.False(output.Success);
        Assert.Equal("Condition 3 should be true", output.Errors.First());
    }
}
