#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     An OpenWire command
    /// </summary>
    public interface IDataStructure : ICloneable
    {
        Byte GetDataStructureType();
    }
}