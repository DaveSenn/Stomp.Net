#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Represents a text based message
    /// </summary>
    public interface ITextMessage : IMessage
    {
        #region Properties

        String Text { get; set; }

        #endregion
    }
}