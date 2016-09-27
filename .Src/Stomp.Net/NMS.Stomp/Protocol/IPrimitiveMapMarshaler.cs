#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Protocol
{
    /// <summary>
    ///     Interface for a utility class used to marshal an IPrimitiveMap instance
    ///     to/from an String.
    /// </summary>
    public interface IPrimitiveMapMarshaler
    {
        #region Properties

        /// <summary>
        ///     Retrieves the Name of this Marshaler.
        /// </summary>
        String Name { get; }

        #endregion

        /// <summary>
        ///     Marshals a PrimitiveMap instance to an serialized byte array.
        /// </summary>
        Byte[] Marshal( IPrimitiveMap map );

        /// <summary>
        ///     Un-marshals an IPrimitiveMap instance from a String object.
        /// </summary>
        IPrimitiveMap Unmarshal( Byte[] mapContent );
    }
}