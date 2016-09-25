

#region Usings

using System;
using System.Threading;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Define an enumerated array of message priorities.
    /// </summary>
    public enum MsgPriority
    {
        Lowest = 0,
        VeryLow = 1,
        Low = 2,
        AboveLow = 3,
        BelowNormal = 4,
        Normal = 5,
        AboveNormal = 6,
        High = 7,
        VeryHigh = 8,
        Highest = 9
    }

    /// <summary>
    ///     Define an enumerated array of message delivery modes.  Provider-specific
    ///     values can be used to extend this enumerated mode.  TIBCO is known to
    ///     provide a third value of ReliableDelivery.  At minimum, a provider must
    ///     support Persistent and NonPersistent.
    /// </summary>
    public enum MsgDeliveryMode
    {
        Persistent,
        NonPersistent
    }

    /// <summary>
    ///     Defines a number of constants
    /// </summary>
    public class NMSConstants
    {
        #region Constants

        public const MsgDeliveryMode defaultDeliveryMode = MsgDeliveryMode.Persistent;
        public const MsgPriority defaultPriority = MsgPriority.Normal;
        public static readonly TimeSpan defaultRequestTimeout = TimeSpan.FromMilliseconds( Timeout.Infinite );
        public static readonly TimeSpan defaultTimeToLive = TimeSpan.Zero;

        #endregion
    }
}