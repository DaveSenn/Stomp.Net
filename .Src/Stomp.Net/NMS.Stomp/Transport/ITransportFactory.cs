

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public delegate void SetTransport( ITransport transport, Uri uri );

    public interface ITransportFactory
    {
        ITransport CompositeConnect( Uri location );
        ITransport CompositeConnect( Uri location, SetTransport setTransport );
        ITransport CreateTransport( Uri location );
    }
}