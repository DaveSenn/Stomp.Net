#region Usings

using System;

#endregion

namespace Stomp.Net.Example.SendReceiveCore;

/// <summary>
///     Console logger for Stomp.Net
/// </summary>
public class ConsoleLogger : ITrace
{
    #region Implementation of ITrace

    /// <summary>
    ///     Gets a value indicating whether the error level is enabled or not.
    /// </summary>
    public Boolean IsErrorEnabled => true;

    /// <summary>
    ///     Gets a value indicating whether the warn level is enabled or not.
    /// </summary>
    public Boolean IsWarnEnabled => true;

    /// <summary>
    ///     Gets a value indicating whether the info level is enabled or not.
    /// </summary>
    public Boolean IsInfoEnabled => true;

    /// <summary>
    ///     Gets a value indicating whether the fatal level is enabled or not.
    /// </summary>
    public Boolean IsFatalEnabled => true;

    /// <summary>
    ///     Gets a value indicating whether the debug level is enabled or not.
    /// </summary>
    public Boolean IsDebugEnabled => true;

    /// <summary>
    ///     Writes a message on the error level.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Error( String message )
        => Console.WriteLine( $"[Error]\t{message}" );

    /// <summary>
    ///     Writes a message on the fatal level.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Fatal( String message )
        => Console.WriteLine( $"[Fatal]\t{message}" );

    /// <summary>
    ///     Writes a message on the info level.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Info( String message )
        => Console.WriteLine( $"[Info]\t{message}" );

    /// <summary>
    ///     Writes a message on the debug level.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Debug( String message )
        => Console.WriteLine( $"[Debug]\t{message}" );

    /// <summary>
    ///     Writes a message on the warn level.
    /// </summary>
    /// <param name="message">The message.</param>
    public void Warn( String message )
        => Console.WriteLine( $"[Warn]\t{message}" );

    #endregion
}