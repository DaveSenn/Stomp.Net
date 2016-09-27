#region Usings

using System;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Defines a number of constants
    /// </summary>
    public class NmsConstants
    {
        #region Constants

        public const MessageDeliveryMode DefaultDeliveryMode = MessageDeliveryMode.Persistent;
        public const MessagePriority DefaultPriority = MessagePriority.Normal;
        public static readonly TimeSpan DefaultTimeToLive = TimeSpan.Zero;

        #endregion
    }
}