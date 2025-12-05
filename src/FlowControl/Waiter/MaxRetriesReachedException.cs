using ArturRios.Output;

namespace ArturRios.Util.FlowControl.Waiter;

/// <summary>
/// Exception thrown when a retry helper exceeds its configured maximum retry count.
/// </summary>
/// <param name="messages">Optional custom messages describing the failure; a default message is used if omitted.</param>
public class MaxRetriesReachedException(string[]? messages = null)
    : CustomException(messages ?? ["Maximum retry count exceeded"]);
