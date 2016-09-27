#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Class representing a byte message.
    /// </summary>
    public class BytesMessage : Message, IBytesMessage
    {
        #region Implementation of IBytesMessage

        /// <summary>
        ///     Gets the length of the message content.
        /// </summary>
        /// <value>The length of the message content.</value>
        public Int64 ContentLength => Content.Length;

        #endregion

        #region Override of Message

        public override Byte GetDataStructureType()
            => DataStructureTypes.BytesMessageType;

        #endregion
    }
}