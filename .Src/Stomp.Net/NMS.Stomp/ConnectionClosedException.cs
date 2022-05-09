#region Usings

#endregion

namespace Stomp.Net.Stomp;

/// <summary>
///     Exception thrown when a connection is used that it already closed
/// </summary>
public class ConnectionClosedException : StompException
{
    #region Ctor

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectionClosedException" /> class.
    /// </summary>
    public ConnectionClosedException()
        : base( "The connection is already closed!" )
    {
    }

    #endregion
}