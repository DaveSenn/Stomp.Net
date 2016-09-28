#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A BytesMessage object is used to send a message containing a stream of uninterpreted
    ///     bytes. It inherits from the Message interface and adds a bytes message body. The
    ///     receiver of the message supplies the interpretation of the bytes.
    ///     This message type is for client encoding of existing message formats. If possible,
    ///     one of the other self-defining message types should be used instead.
    ///     Although the NMS API allows the use of message properties with byte messages, they
    ///     are typically not used, since the inclusion of properties may affect the format.
    ///     When the message is first created, and when ClearBody is called, the body of the
    ///     message is in write-only mode. After the first call to Reset has been made, the
    ///     message body is in read-only mode. After a message has been sent, the client that
    ///     sent it can retain and modify it without affecting the message that has been sent.
    ///     The same message object can be sent multiple times. When a message has been received,
    ///     the provider has called Reset so that the message body is in read-only mode for the
    ///     client.
    ///     If ClearBody is called on a message in read-only mode, the message body is cleared and
    ///     the message is in write-only mode.
    ///     If a client attempts to read a message in write-only mode, a MessageNotReadableException
    ///     is thrown.
    ///     If a client attempts to write a message in read-only mode, a MessageNotWriteableException
    ///     is thrown.
    /// </summary>
    public interface IBytesMessage : IMessage
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the message content.
        /// </summary>
        /// <value>The message content.</value>
        Byte[] Content { get; set; }

        /// <summary>
        ///     Gets the length of the message content.
        /// </summary>
        /// <value>The length of the message content.</value>
        Int64 ContentLength { get; }

        #endregion
    }
}