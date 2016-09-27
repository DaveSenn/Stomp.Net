#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    public class DateUtils
    {
        #region Constants

        /// <summary>
        ///     The difference between the Windows epoch and the Java epoch
        ///     in milliseconds.
        /// </summary>
        public static readonly Int64 epochDiff; /* = 1164447360000L; */

        /// <summary>
        ///     The start of the Java epoch
        /// </summary>
        public static readonly DateTime javaEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );

        /// <summary>
        ///     The start of the Windows epoch
        /// </summary>
        public static readonly DateTime windowsEpoch = new DateTime( 1601, 1, 1, 0, 0, 0, 0 );

        #endregion

        #region Ctor

        static DateUtils()
        {
            epochDiff = ( javaEpoch.ToFileTimeUtc() - windowsEpoch.ToFileTimeUtc() )
                        / TimeSpan.TicksPerMillisecond;
        }

        #endregion

        public static DateTime ToDateTime( Int64 javaTime ) => DateTime.FromFileTime( ( javaTime + epochDiff ) * TimeSpan.TicksPerMillisecond );

        public static DateTime ToDateTimeUtc( Int64 javaTime ) => DateTime.FromFileTimeUtc( ( javaTime + epochDiff ) * TimeSpan.TicksPerMillisecond );
        
        public static Int64 ToJavaTimeUtc( DateTime dateTime ) => dateTime.ToFileTimeUtc() / TimeSpan.TicksPerMillisecond - epochDiff;
    }
}