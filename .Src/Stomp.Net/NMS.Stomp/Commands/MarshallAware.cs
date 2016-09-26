#region Usings

using System;
using Apache.NMS.Stomp.Protocol;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Represents a marshallable entity
    /// </summary>
    public interface MarshallAware
    {
        void AfterMarshall( StompWireFormat wireFormat );
        void AfterUnmarshall( StompWireFormat wireFormat );
        void BeforeMarshall( StompWireFormat wireFormat );

        void BeforeUnmarshall( StompWireFormat wireFormat );
        Byte[] GetMarshalledForm( StompWireFormat wireFormat );

        void SetMarshalledForm( StompWireFormat wireFormat, Byte[] data );
    }
}