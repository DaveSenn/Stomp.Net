#region Usings

using System;
using System.IO;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport;

/// <summary>
///     Represents the marshaling of commands to and from an IO stream
/// </summary>
public interface IWireFormat
{
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