#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Threads
{
    /// <summary>
    ///     Represents a task that may take a few iterations to complete.
    /// </summary>
    public interface ITask
    {
        Boolean Iterate();
    }
}