#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Exception thrown when an IO error occurs
    /// </summary>
    [Serializable]
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