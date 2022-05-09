#region Usings

using System;

#endregion

namespace Stomp.Net;

public class IllegalStateException : StompException
{
    #region Ctor

    public IllegalStateException( String message )
        : base( message )
    {
    }

    #endregion
}