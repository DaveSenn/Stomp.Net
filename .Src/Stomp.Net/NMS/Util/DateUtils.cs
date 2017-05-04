#region Usings

using System;

#endregion

namespace Stomp.Net.Util
{
    /// <summary>
    ///     Apache MQ related converter class. This class is Apache MQ specific :(
    /// </summary>
    public class DateUtils
    {
        #region Constants

        /// <summary>
        ///     The difference between the Windows epoch and the Java epoch
        ///     in milliseconds.
        /// </summary>
        private static readonly Int64 EpochDiff; /* = 1164447360000L; */

        /// <summary>
        ///     The start of the Java epoch
        /// </summary>
        private static readonly DateTime JavaEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );

        /// <summary>
        ///     The start of the Windows epoch
        /// </summary>
        private static readonly DateTime WindowsEpoch = new DateTime( 1601, 1, 1, 0, 0, 0, 0 );

        #endregion

        #region Ctor

        static DateUtils() => EpochDiff = ( JavaEpoch.ToFileTimeUtc() - WindowsEpoch.ToFileTimeUtc() ) / TimeSpan.TicksPerMillisecond;

        #endregion

        public static DateTime ToDateTime( Int64 javaTime )
            => DateTime.FromFileTime( ( javaTime + EpochDiff ) * TimeSpan.TicksPerMillisecond );

        public static DateTime ToDateTimeUtc( Int64 javaTime )
            => DateTime.FromFileTimeUtc( ( javaTime + EpochDiff ) * TimeSpan.TicksPerMillisecond );

        public static Int64 ToJavaTimeUtc( DateTime dateTime )
            => dateTime.ToFileTimeUtc() / TimeSpan.TicksPerMillisecond - EpochDiff;
    }
}