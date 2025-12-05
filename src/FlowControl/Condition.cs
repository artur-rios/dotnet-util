using ArturRios.Output;

namespace ArturRios.Util.FlowControl;

/// <summary>
/// A fluent helper to evaluate multiple boolean conditions and aggregate failures.
/// </summary>
/// <remarks>
/// Use <see cref="True"/> or <see cref="False"/> to set the result of an expression and chain <see cref="FailsWith"/> to capture error messages.
/// After building all conditions call <see cref="ThrowIfNotSatisfied"/> or inspect <see cref="IsSatisfied"/> / <see cref="FailedConditions"/>.
/// </remarks>
public class Condition
{
    private readonly HashSet<string> _failedConditions = [];
    private bool _expression;

    /// <summary>
    /// Creates a new <see cref="Condition"/> instance. Syntactic sugar for <c>new Condition()</c>.
    /// </summary>
    public static Condition Create => new();

    /// <summary>
    /// Gets an array of all failure messages collected by <see cref="FailsWith"/>.
    /// </summary>
    public string[] FailedConditions => _failedConditions.ToArray();

    /// <summary>
    /// Indicates whether all evaluated conditions have succeeded (i.e. there are no failures).
    /// </summary>
    public bool IsSatisfied => _failedConditions.Count == 0;

    /// <summary>
    /// Sets the current expression result to <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The boolean value representing the success of the condition.</param>
    /// <returns>The current <see cref="Condition"/> for fluent chaining.</returns>
    public Condition True(bool expression)
    {
        _expression = expression;

        return this;
    }

    /// <summary>
    /// Sets the current expression result to the negated value of <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The boolean to negate.</param>
    /// <returns>The current <see cref="Condition"/> for fluent chaining.</returns>
    public Condition False(bool expression)
    {
        _expression = !expression;

        return this;
    }

    /// <summary>
    /// Adds an error message if the most recently set expression evaluated to <c>false</c>.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>The current <see cref="Condition"/> for fluent chaining.</returns>
    public Condition FailsWith(string error)
    {
        if (!_expression)
        {
            _failedConditions.Add(error);
        }

        return this;
    }

    /// <summary>
    /// Throws a <see cref="ConditionFailedException"/> if any condition failed.
    /// </summary>
    /// <exception cref="ConditionFailedException">Thrown when <see cref="IsSatisfied"/> is false.</exception>
    public void ThrowIfNotSatisfied()
    {
        if (IsSatisfied)
        {
            return;
        }

        throw new ConditionFailedException(FailedConditions);
    }

    /// <summary>
    /// Converts the condition failures into a <see cref="ProcessOutput"/> instance.
    /// </summary>
    /// <returns>A <see cref="ProcessOutput"/> with errors populated when the condition is not satisfied.</returns>
    public ProcessOutput ToProcessOutput()
    {
        var output = new ProcessOutput();

        if (!IsSatisfied)
        {
            output.AddErrors(_failedConditions.ToList());
        }

        return output;
    }
}

/// <summary>
/// Exception raised when one or more conditions fail in a <see cref="Condition"/> chain.
/// </summary>
/// <param name="errors">The set of failure messages collected.</param>
public class ConditionFailedException(string[] errors) : Exception($"A total of {errors.Length} conditions failed")
{
    /// <summary>
    /// The failure messages that caused this exception.
    /// </summary>
    public readonly string[] Errors = errors;
}
