#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands;

public class ConnectionInfo : BaseCommand
{
    /// <summery>
    ///     Get the unique identifier that this object and its own
    ///     Marshaler share.
    /// </summery>
    public override Byte GetDataStructureType()
        => DataStructureTypes.ConnectionInfoType;

    /// <summery>
    ///     Returns a string containing the information for this DataStructure
    ///     such as its type and value of its elements.
    /// </summery>
    public override String ToString()
        => GetType()
               .Name + "[" +
           "ConnectionId=" + ConnectionId + ", " +
           "Host=" + Host + ", " +
           "ClientId=" + ClientId + ", " +
           "Password=" + Password + ", " +
           "UserName=" + UserName +
           "]";

    #region Properties

    public ConnectionId ConnectionId { get; set; }

    public String Host { get; set; }

    public String ClientId { get; set; }

    public String Password { get; set; }

    public String UserName { get; set; }

    /// <summery>
    ///     Return an answer of true to the isConnectionInfo() query.
    /// </summery>
    public override Boolean IsConnectionInfo => true;

    #endregion
}