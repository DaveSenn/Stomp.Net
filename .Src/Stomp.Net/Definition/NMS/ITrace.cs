#region Usings

using System;

#endregion

namespace Stomp.Net
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
        /// <summary>
        ///     Writes a message on the error level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error( String message );

        /// <summary>
        ///     Writes a message on the fatal level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Fatal( String message );

        /// <summary>
        ///     Writes a message on the info level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info( String message );

        /// <summary>
        ///     Writes a message on the warn level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warn( String message );
    }
}