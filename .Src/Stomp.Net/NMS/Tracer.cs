#region Usings

using System;
using System.Runtime.CompilerServices;

#endregion

namespace Stomp.Net
{
    public static class Tracer
    {
        /// <summary>
        ///     Gets a value indicating whether the error level is enabled or not.
        /// </summary>
        public static Boolean IsErrorEnabled => Trace?.IsErrorEnabled == true;

        /// <summary>
        ///     Gets a value indicating whether the warn level is enabled or not.
        /// </summary>
        public static Boolean IsWarnEnabled => Trace?.IsWarnEnabled == true;

        /// <summary>
        ///     Gets a value indicating whether the info level is enabled or not.
        /// </summary>
        public static Boolean IsInfoEnabled => Trace?.IsInfoEnabled == true;

        /// <summary>
        ///     Gets a value indicating whether the fatal level is enabled or not.
        /// </summary>
        public static Boolean IsFatalEnabled => Trace?.IsFatalEnabled == true;

        /// <summary>
        ///     Gets a value indicating whether the debug level is enabled or not.
        /// </summary>
        public static Boolean IsDebugEnabled => Trace?.IsDebugEnabled == true;

        public static void Debug( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 callerLineNumber = 0 )
        {
            if ( AddCallerInfo )
                Trace?.Debug( message + $"    ({callerName} Ln {callerLineNumber} [{callerFilePath}])" );
            else
                Trace?.Debug( message );
        }

        public static void Error( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 callerLineNumber = 0 )
        {
            if ( AddCallerInfo )
                Trace?.Error( message + $"    ({callerName} Ln {callerLineNumber} [{callerFilePath}])" );
            else
                Trace?.Error( message );
        }

        public static void Fatal( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 callerLineNumber = 0 )
        {
            if ( AddCallerInfo )
                Trace?.Fatal( message + $"    ({callerName} Ln {callerLineNumber} [{callerFilePath}])" );
            else
                Trace?.Fatal( message );
        }

        public static void Info( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 callerLineNumber = 0 )
        {
            if ( AddCallerInfo )
                Trace?.Info( message + $"    ({callerName} Ln {callerLineNumber} [{callerFilePath}])" );
            else
                Trace?.Info( message );
        }

        public static void Warn( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 callerLineNumber = 0 )
        {
            if ( AddCallerInfo )
                Trace?.Warn( message + $"    ({callerName} Ln {callerLineNumber} [{callerFilePath}])" );
            else
                Trace?.Warn( message );
        }

        #region Properties

        /// <summary>
        ///     Gets or sets a <see cref="ITrace" />.
        /// </summary>
        /// <value>A <see cref="ITrace" />.</value>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static ITrace Trace { get; set; }

        /// <summary>
        ///     Gets or sets a value determining whether the caller info will be added to the log output or not.
        /// </summary>
        /// <value>A value determining whether the caller info will be added to the log output or not.</value>
        public static Boolean AddCallerInfo { get; set; } = true;

        #endregion
    }
}