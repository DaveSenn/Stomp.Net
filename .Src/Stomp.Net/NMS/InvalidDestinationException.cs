#region Usings

using System;

#endregion

namespace Stomp.Net;

/// <summary>
///     An attempt is made to access an invalid destination
/// </summary>
public class InvalidDestinationException : StompException
{
    #region Ctor

    public InvalidDestinationException( String message )
        : base( message )
    {
    }

    #endregion
}