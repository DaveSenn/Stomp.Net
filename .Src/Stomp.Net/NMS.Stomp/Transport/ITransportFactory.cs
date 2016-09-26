#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public interface ITransportFactory
    {
        ITransport CompositeConnect( Uri location );
        ITransport CreateTransport( Uri location );
    }
}