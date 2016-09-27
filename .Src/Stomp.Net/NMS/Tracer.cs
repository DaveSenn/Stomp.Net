#region Usings

using System;
using Extend;

#endregion

namespace Apache.NMS
{
    public static class Tracer
    {
        #region Properties

        public static ITrace Trace { get; set; } = null;

        public static Boolean IsDebugEnabled
        {
            get { return Trace != null && Trace.IsDebugEnabled; }
        }

        public static Boolean IsInfoEnabled
        {
            get { return Trace != null && Trace.IsInfoEnabled; }
        }

        public static Boolean IsWarnEnabled
        {
            get { return Trace != null && Trace.IsWarnEnabled; }
        }

        public static Boolean IsErrorEnabled
        {
            get { return Trace != null && Trace.IsErrorEnabled; }
        }

        public static Boolean IsFatalEnabled
        {
            get { return Trace != null && Trace.IsFatalEnabled; }
        }

        #endregion

        public static void Error( Object message )
        {
            if ( IsErrorEnabled )
                Trace.Error( message.ToString() );
        }

        public static void ErrorFormat( String format, params Object[] args )
        {
            if ( IsErrorEnabled )
                Trace.Error( format.F( args ) );
        }

        public static void Fatal( Object message )
        {
            if ( IsFatalEnabled )
                Trace.Fatal( message.ToString() );
        }

        public static void Info( Object message )
        {
            if ( IsInfoEnabled )
                Trace.Info( message.ToString() );
        }

        public static void InfoFormat( String format, params Object[] args )
        {
            if ( IsInfoEnabled )
                Trace.Info( format.F( args ) );
        }

        public static void Warn( Object message )
        {
            if ( IsWarnEnabled )
                Trace.Warn( message.ToString() );
        }

        public static void WarnFormat( String format, params Object[] args )
        {
            if ( IsWarnEnabled )
                Trace.Warn( format.F( args ) );
        }
    }
}