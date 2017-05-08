#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when a connection is used that it already closed
    /// </summary>
    public class ConnectionClosedException : StompException
    {
        #region Ctor

        public ConnectionClosedException()
            : base( "The connection is already closed!" )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message )
            : base( message )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, String errorCode )
            : base( message, errorCode )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, Exception innerException )
            : base( message, innerException )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public ConnectionClosedException( String message, String errorCode, Exception innerException )
            : base( message, errorCode, innerException )
        {
        }
        
        #endregion
    }
}