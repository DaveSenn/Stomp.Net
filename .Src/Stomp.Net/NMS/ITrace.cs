#region Usings

using System;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     The ITrace interface is used internally by ActiveMQ to log messages.
    ///     The client application may provide an implementation of ITrace if it wishes to
    ///     route messages to a specific destination.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="Tracer" /> class to register an instance of ITrace as the
    ///         active trace destination.
    ///     </para>
    /// </remarks>
    public interface ITrace
    {
        void Error( String message );
        void Fatal( String message );
        void Info( String message );
        void Warn( String message );
    }
}