#region Usings

using System;

#endregion

namespace Stomp.Net
{
    public interface ITransportFactory
    {
        ITransport CompositeConnect( Uri location );
        ITransport CreateTransport( Uri location );
    }
}