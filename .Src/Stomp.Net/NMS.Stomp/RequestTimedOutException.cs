#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when an Request times out.
    /// </summary>
    public class RequestTimedOutException : IoException
    {
        #region Ctor

        public RequestTimedOutException( String msg )
            : base( msg )
        {
        }

        // ReSharper disable once UnusedMember.Global
        public RequestTimedOutException( String msg, Exception inner )
            : base( msg, inner )
        {
        }

        #endregion
    }
}