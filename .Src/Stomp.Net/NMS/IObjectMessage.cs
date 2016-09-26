#region Usings

using System;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents an Object message which contains a serializable .Net object.
    /// </summary>
    public interface IObjectMessage : IMessage
    {
        #region Properties

        Object Body { get; set; }

        #endregion
    }
}