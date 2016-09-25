

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    public interface ICompositeTransport : ITransport
    {
        void Add( Uri[] uris );
        void Remove( Uri[] uris );
    }
}