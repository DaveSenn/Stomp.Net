

#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     Represents the marshalling of commands to and from an IO stream
    /// </summary>
    public interface IWireFormat
    {
        #region Properties

        ITransport Transport { get; set; }

        #endregion

        /// <summary>
        ///     Marshalls the given command object onto the stream
        /// </summary>
        void Marshal( Object o, BinaryWriter ds );

        /// <summary>
        ///     Unmarshalls the next command object from the stream
        /// </summary>
        Object Unmarshal( BinaryReader dis );
    }
}