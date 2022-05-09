#region Usings

using System;

#endregion

namespace Stomp.Net;

public interface ITransportFactory
{
    ITransport CreateTransport( Uri location );
}