#region Usings

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace Apache.NMS.Stomp.Util
{
    public class IdGenerator
    {
        #region Constants

        private static Int32 instanceCount;
        private static readonly String UNIQUE_STUB;

        #endregion

        #region Fields

        private readonly String seed;
        private Int64 sequence;

        #endregion

        #region Properties

        /// <summary>
        ///     As we have to find the hostname as a side-affect of generating a unique
        ///     stub, we allow it's easy retrevial here
        /// </summary>
        public static String HostName { get; }

        #endregion

        #region Ctor

        static IdGenerator()
        {
            var stub = "-1-" + DateTime.Now.Ticks;
            HostName = "localhost";

            try
            {
                HostName = Dns.GetHostName();
                var endPoint = new IPEndPoint( IPAddress.Any, 0 );
                var tempSocket = new Socket( endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
                tempSocket.Bind( endPoint );
                stub = "-" + ( (IPEndPoint) tempSocket.LocalEndPoint ).Port + "-" + DateTime.Now.Ticks + "-";
                Thread.Sleep( 100 );
                tempSocket.Close();
            }
            catch ( Exception ioe )
            {
                Tracer.Warn( "could not generate unique stub: " + ioe.Message );
            }

            UNIQUE_STUB = stub;
        }

        /**
         * Construct an IdGenerator
         */

        public IdGenerator( String prefix )
        {
            lock ( UNIQUE_STUB )
                seed = prefix + UNIQUE_STUB + instanceCount++ + ":";
        }

        public IdGenerator()
            : this( "ID:" + HostName )
        {
        }

        #endregion

        /// <summary>
        ///     Does a proper compare on the ids
        /// </summary>
        public static Int32 Compare( String id1, String id2 )
        {
            var result = -1;

            var seed1 = GetSeedFromId( id1 );
            var seed2 = GetSeedFromId( id2 );

            if ( seed1 != null && seed2 != null )
            {
                result = seed1.CompareTo( seed2 );

                if ( result == 0 )
                {
                    var count1 = GetSequenceFromId( id1 );
                    var count2 = GetSequenceFromId( id2 );
                    result = (Int32) ( count1 - count2 );
                }
            }

            return result;
        }

        /// <summary>
        ///     Generate a Unique Id
        /// </summary>
        public String GenerateId()
        {
            lock ( UNIQUE_STUB )
                return seed + sequence++;
        }

        /// <summary>
        ///     Generate a unique ID - that is friendly for a URL or file system
        /// </summary>
        public String GenerateSanitizedId()
        {
            var result = GenerateId();
            result = result.Replace( ':', '-' );
            result = result.Replace( '_', '-' );
            result = result.Replace( '.', '-' );
            return result;
        }

        /// <summary>
        ///     From a generated id - return the seed (i.e. minus the count)
        /// </summary>
        public static String GetSeedFromId( String id )
        {
            var result = id;

            if ( id != null )
            {
                var index = id.LastIndexOf( ':' );
                if ( index > 0 && index + 1 < id.Length )
                    result = id.Substring( 0, index + 1 );
            }

            return result;
        }

        /// <summary>
        ///     From a generated id - return the generator count
        /// </summary>
        public static Int64 GetSequenceFromId( String id )
        {
            Int64 result = -1;
            if ( id != null )
            {
                var index = id.LastIndexOf( ':' );

                if ( index > 0 && index + 1 < id.Length )
                {
                    var numStr = id.Substring( index + 1, id.Length );
                    result = Int64.Parse( numStr );
                }
            }
            return result;
        }
    }
}