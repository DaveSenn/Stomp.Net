#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    public interface ICompositeTransport : ITransport
    {
        [PublicAPI]
        void Add( Uri[] uris );

        [PublicAPI]
        void Remove( Uri[] uris );
    }
}