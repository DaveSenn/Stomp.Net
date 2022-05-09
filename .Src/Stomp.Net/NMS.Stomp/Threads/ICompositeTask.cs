#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Threads;

/// <summary>
///     A Composite task is one of N tasks that can be managed by a
///     CompositTaskRunner instance.  The CompositeTaskRunner checks each
///     task when its wakeup method is called to determine if the Task has
///     any work it needs to complete, if no tasks have any pending work
///     then the CompositeTaskRunner can return to its sleep state until
///     the next time its wakeup method is called or it is shut down.
/// </summary>
public interface ICompositeTask : ITask
{
    #region Properties

    /// <summary>
    ///     Indicates if this Task has any pending work.
    /// </summary>
    Boolean IsPending { get; }

    #endregion
}