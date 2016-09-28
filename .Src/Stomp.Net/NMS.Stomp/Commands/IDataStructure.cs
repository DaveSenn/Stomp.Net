#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     An OpenWire command
    /// </summary>
    public interface IDataStructure : ICloneable
    {
        Byte GetDataStructureType();
    }
}