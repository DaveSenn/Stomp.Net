#region Usings

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Extend;

#endregion

namespace Stomp.Net
{
    public static class Tracer
    {
        #region Properties

        /// <summary>
        ///     Gets or sets a <see cref="ITrace" />.
        /// </summary>
        /// <value>A <see cref="ITrace" />.</value>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public static ITrace Trace { get; set; } = null;

        #endregion

        public static void Error( Object message )
            => Trace?.Error( message.ToString() );

        public static void ErrorFormat( String format, params Object[] args )
            => Trace?.Error( format.F( args ) );

        public static void Fatal( Object message )
            => Trace?.Fatal( message.ToString() );

        public static void Info( Object message )
            => Trace?.Info( message.ToString() );

        public static void InfoFormat( String format, params Object[] args )
            => Trace?.Info( format.F( args ) );

        public static void Warn( Object message )
        {
            Trace?.Warn( message.ToString() );
        }

        public static void WarnFormat( String format, params Object[] args )
        {
            //var callerInfo = GetCallerInfo();
            //Trace?.Warn( format.F( args ) + "\t" + callerInfo );
            Trace?.Warn( format.F( args ) );
        }

        /*
        private static String GetCallerInfo()
        {
            var stackTrace = new StackTrace();

            var callerFrame = stackTrace.GetFrame( 2 );
            var callingMethod = callerFrame.GetMethod();

            return $"{callingMethod?.DeclaringType?.Name}.{callingMethod?.Name}";
        }
        */
        private static String Test( String message, [CallerMemberName] String callerName = "", [CallerFilePath] String callerFilePath = "", [CallerLineNumber] Int32 CallerLineNumber = 0 )
            => message;
    }
}