

#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     Support class that switches from one endian to the other.
    /// </summary>
    [CLSCompliant( false )]
    public class EndianSupport
    {
        public static Char SwitchEndian( Char x ) => (Char) (
            ( (Char) (Byte) x << 8 ) |
            (Char) (Byte) ( x >> 8 )
        );

        public static Int16 SwitchEndian( Int16 x ) => (Int16) (
            ( (Byte) x << 8 ) |
            (Byte) ( x >> 8 )
        );

        public static Int32 SwitchEndian( Int32 x ) => ( (Byte) x << 24 ) |
                                                       ( (Byte) ( x >> 8 ) << 16 ) |
                                                       ( (Byte) ( x >> 16 ) << 8 ) |
                                                       (Byte) ( x >> 24 );

        public static Int64 SwitchEndian( Int64 x ) => ( (Int64) (Byte) x << 56 ) |
                                                       ( (Int64) (Byte) ( x >> 8 ) << 48 ) |
                                                       ( (Int64) (Byte) ( x >> 16 ) << 40 ) |
                                                       ( (Int64) (Byte) ( x >> 24 ) << 32 ) |
                                                       ( (Int64) (Byte) ( x >> 32 ) << 24 ) |
                                                       ( (Int64) (Byte) ( x >> 40 ) << 16 ) |
                                                       ( (Int64) (Byte) ( x >> 48 ) << 8 ) |
                                                       (Byte) ( x >> 56 );

        public static UInt16 SwitchEndian( UInt16 x ) => (UInt16) (
            ( (Byte) x << 8 ) |
            (Byte) ( x >> 8 )
        );

        public static UInt32 SwitchEndian( UInt32 x ) => ( (UInt32) (Byte) x << 24 ) |
                                                         ( (UInt32) (Byte) ( x >> 8 ) << 16 ) |
                                                         ( (UInt32) (Byte) ( x >> 16 ) << 8 ) |
                                                         (Byte) ( x >> 24 );

        public static UInt64 SwitchEndian( UInt64 x ) => ( (UInt64) (Byte) x << 56 ) |
                                                         ( (UInt64) (Byte) ( x >> 8 ) << 48 ) |
                                                         ( (UInt64) (Byte) ( x >> 16 ) << 40 ) |
                                                         ( (UInt64) (Byte) ( x >> 24 ) << 32 ) |
                                                         ( (UInt64) (Byte) ( x >> 32 ) << 24 ) |
                                                         ( (UInt64) (Byte) ( x >> 40 ) << 16 ) |
                                                         ( (UInt64) (Byte) ( x >> 48 ) << 8 ) |
                                                         (Byte) ( x >> 56 );

        public static Double SwitchEndian( Double x )
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter( ms );
            bw.Write( x );
            bw.Flush();
            ms = new MemoryStream( SwitchEndian( ms.ToArray() ) );
            var br = new BinaryReader( ms );
            return br.ReadDouble();
        }

        public static Single SwitchEndian( Single x )
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter( ms );
            bw.Write( x );
            bw.Flush();
            ms = new MemoryStream( SwitchEndian( ms.ToArray() ) );
            var br = new BinaryReader( ms );
            return br.ReadSingle();
        }

        public static Byte[] SwitchEndian( Byte[] x )
        {
            var rc = new Byte[x.Length];
            var j = x.Length - 1;
            for ( var i = 0; i < x.Length; i++ )
            {
                rc[i] = x[j];
                j--;
            }
            return rc;
        }
    }
}