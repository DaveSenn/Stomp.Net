

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Exception thrown when an Request times out.
    /// </summary>
    public class RequestTimedOutException : IOException
    {
        #region Ctor

        public RequestTimedOutException()
            : base( "IO Exception failed with missing exception log" )
        {
        }

        public RequestTimedOutException( String msg )
            : base( msg )
        {
        }

        public RequestTimedOutException( String msg, Exception inner )
            : base( msg, inner )
        {
        }

        #endregion
    }
}