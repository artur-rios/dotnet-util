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

    [Fact]
    public void GivenTwoConditionsFailingWithTheSameMessage_WhenFailedConditions_ThenBothAreReported()
    {
        // A HashSet used to collapse these into a single entry, so the reported count undercounted.
        var condition = Condition.Create
            .True(false).FailsWith("Value is required")
            .True(false).FailsWith("Value is required");

        Assert.Equal(2, condition.FailedConditions.Length);
    }

    [Fact]
    public void GivenFailuresInOrder_WhenFailedConditions_ThenOrderIsPreserved()
    {
        var condition = Condition.Create
            .True(false).FailsWith("first")
            .True(false).FailsWith("second")
            .True(false).FailsWith("third");

        Assert.Equal(["first", "second", "third"], condition.FailedConditions);
    }

    [Fact]
    public void GivenNoExpression_WhenFailsWith_ThenThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Condition.Create.FailsWith("no expression was set"));
    }

    [Fact]
    public void GivenNullError_WhenFailsWith_ThenThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Condition.Create.True(false).FailsWith(null!));
    }

    [Fact]
    public void GivenSatisfiedCondition_WhenThrowIfNotSatisfied_ThenDoNotThrow()
    {
        var condition = Condition.Create.True(true).FailsWith("should not fail");

        Assert.True(condition.IsSatisfied);
        Assert.Empty(condition.FailedConditions);

        condition.ThrowIfNotSatisfied();
    }

    [Fact]
    public void GivenFailedCondition_WhenThrowIfNotSatisfied_ThenThrowWithEveryMessage()
    {
        var condition = Condition.Create
            .True(false).FailsWith("first failure")
            .False(true).FailsWith("second failure");

        var exception = Assert.Throws<ConditionFailedException>(condition.ThrowIfNotSatisfied);

        Assert.Equal(["first failure", "second failure"], exception.Errors);
        Assert.Contains("first failure", exception.Message);
        Assert.Contains("second failure", exception.Message);
        Assert.Contains("2", exception.Message);
    }

    [Fact]
    public void GivenFalseHelper_WhenExpressionIsTrue_ThenRecordFailure()
    {
        var condition = Condition.Create.False(true).FailsWith("must not be true");

        Assert.False(condition.IsSatisfied);
        Assert.Equal(["must not be true"], condition.FailedConditions);
    }

    [Fact]
    public void GivenFalseHelper_WhenExpressionIsFalse_ThenRecordNothing()
    {
        var condition = Condition.Create.False(false).FailsWith("must not be true");

        Assert.True(condition.IsSatisfied);
    }

    [Fact]
    public void GivenOneExpression_WhenFailsWithCalledTwice_ThenBothMessagesAreRecorded()
    {
        var condition = Condition.Create
            .True(false)
            .FailsWith("first reason")
            .FailsWith("second reason");

        Assert.Equal(["first reason", "second reason"], condition.FailedConditions);
    }

    [Fact]
    public void GivenFailedConditions_WhenMutatingTheReturnedArray_ThenTheConditionIsUnaffected()
    {
        var condition = Condition.Create.True(false).FailsWith("original");

        condition.FailedConditions[0] = "tampered";

        Assert.Equal(["original"], condition.FailedConditions);
    }
}
