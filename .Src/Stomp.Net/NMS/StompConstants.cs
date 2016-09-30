#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Defines a number of constants
    /// </summary>
    public class StompConstants
    {
        #region Constants

        public const MessageDeliveryMode DefaultDeliveryMode = MessageDeliveryMode.Persistent;
        public const MessagePriority DefaultPriority = MessagePriority.Normal;
        public static readonly TimeSpan DefaultTimeToLive = TimeSpan.Zero;

        #endregion
    }
}