#region Usings

using System;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public interface ICompositeTransport : ITransport
    {
        void Add( Uri[] uris );
        void Remove( Uri[] uris );
    }
}