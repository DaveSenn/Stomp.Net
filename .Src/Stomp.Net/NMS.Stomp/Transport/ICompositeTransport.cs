#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    public interface ICompositeTransport : ITransport
    {
        void Add( Uri[] uris );
        void Remove( Uri[] uris );
    }
}