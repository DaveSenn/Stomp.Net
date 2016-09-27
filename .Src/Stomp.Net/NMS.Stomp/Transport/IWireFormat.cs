#region Usings

using System;
using System.IO;
using Apache.NMS.Stomp.Commands;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     Represents the marshaling of commands to and from an IO stream
    /// </summary>
    public interface IWireFormat
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the transport.
        /// </summary>
        /// <value>The transport.</value>
        ITransport Transport { get; set; }

        #endregion

        /// <summary>
        ///     Marshals the given command object onto the stream
        /// </summary>
        /// <param name="o"></param>
        /// <param name="writer">A binary writer.</param>
        void Marshal( Object o, BinaryWriter writer );

        /// <summary>
        ///     Unmarshals the next command object from the stream
        /// </summary>
        /// <param name="reader">A binary reader.</param>
        ICommand Unmarshal( BinaryReader reader );
    }
}