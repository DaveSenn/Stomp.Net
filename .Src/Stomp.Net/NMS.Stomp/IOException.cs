#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when an IO error occurs
    /// </summary>
    public class IoException : StompException
    {
        #region Ctor

        public IoException( String msg )
            : base( msg )
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public IoException( String msg, Exception inner )
            : base( msg, inner )
        {
        }

        #endregion
    }
}