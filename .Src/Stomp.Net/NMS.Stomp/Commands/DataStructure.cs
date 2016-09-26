#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     An OpenWire command
    /// </summary>
    public interface DataStructure : ICloneable
    {
        Byte GetDataStructureType();
        Boolean IsMarshallAware();
    }
}