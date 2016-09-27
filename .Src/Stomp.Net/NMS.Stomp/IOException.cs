#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Exception thrown when an IO error occurs
    /// </summary>
    public class IoException : NmsException
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