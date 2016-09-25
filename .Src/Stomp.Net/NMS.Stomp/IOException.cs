

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Exception thrown when an IO error occurs
    /// </summary>
    public class IOException : NMSException
    {
        #region Ctor

        public IOException()
            : base( "IO Exception failed with missing exception log" )
        {
        }

        public IOException( String msg )
            : base( msg )
        {
        }

        public IOException( String msg, Exception inner )
            : base( msg, inner )
        {
        }

        #endregion
    }
}