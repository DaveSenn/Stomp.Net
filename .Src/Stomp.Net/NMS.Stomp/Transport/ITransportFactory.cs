#region Usings

using System;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public interface ITransportFactory
    {
        ITransport CompositeConnect( Uri location );
        ITransport CreateTransport( Uri location );
    }
}