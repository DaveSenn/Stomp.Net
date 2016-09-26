#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     A BinaryWriter that switches the endian orientation of the read operations so that they
    ///     are compatible across platforms.
    /// </summary>
    [CLSCompliant( false )]
    public class EndianBinaryReader : BinaryReader
    {
        #region Ctor

        public EndianBinaryReader( Stream input )
            : base( input )
        {
        }

        #endregion

        /// <summary>
        ///     Method Read
        /// </summary>
        /// <returns>An int</returns>
        /// <param name="buffer">A  char[]</param>
        /// <param name="index">An int</param>
        /// <param name="count">An int</param>
        public override Int32 Read( Char[] buffer, Int32 index, Int32 count )
        {
            var size = base.Read( buffer, index, count );
            for ( var i = 0; i < size; i++ )
                buffer[index + i] = EndianSupport.SwitchEndian( buffer[index + i] );
            return size;
        }

        /// <summary>
        ///     Method ReadChar
        /// </summary>
        /// <returns>A char</returns>
        public override Char ReadChar() => (Char) (
            ( (Char) ReadByte() << 8 ) |
            (Char) ReadByte()
        );

        /// <summary>
        ///     Method ReadChars
        /// </summary>
        /// <returns>A char[]</returns>
        /// <param name="count">An int</param>
        public override Char[] ReadChars( Int32 count )
        {
            var rc = base.ReadChars( count );
            if ( rc != null )
                for ( var i = 0; i < rc.Length; i++ )
                    rc[i] = EndianSupport.SwitchEndian( rc[i] );
            return rc;
        }

        public override Double ReadDouble() => EndianSupport.SwitchEndian( base.ReadDouble() );

        /// <summary>
        ///     Method ReadInt16
        /// </summary>
        /// <returns>A short</returns>
        public override Int16 ReadInt16() => EndianSupport.SwitchEndian( base.ReadInt16() );

        /// <summary>
        ///     Method ReadInt32
        /// </summary>
        /// <returns>An int</returns>
        public override Int32 ReadInt32()
        {
            var x = base.ReadInt32();
            var y = EndianSupport.SwitchEndian( x );
            return y;
        }

        /// <summary>
        ///     Method ReadInt64
        /// </summary>
        /// <returns>A long</returns>
        public override Int64 ReadInt64() => EndianSupport.SwitchEndian( base.ReadInt64() );

        public override Single ReadSingle() => EndianSupport.SwitchEndian( base.ReadSingle() );

        /// <summary>
        ///     Method ReadString
        /// </summary>
        /// <returns>A string</returns>
        public override String ReadString() => ReadString16();

        /// <summary>
        ///     Method ReadString16, reads a String value encoded in the Java modified
        ///     UTF-8 format with a length index encoded as a 16bit unsigned short.
        /// </summary>
        /// <returns>A string</returns>
        public String ReadString16()
        {
            Int32 utfLength = ReadUInt16();

            if ( utfLength < 0 )
                return null;
            if ( utfLength == 0 )
                return "";

            return doReadString( utfLength );
        }

        /// <summary>
        ///     Method ReadString32, reads a String value encoded in the Java modified
        ///     UTF-8 format with a length index encoded as a singed integer value.
        /// </summary>
        /// <returns>A string</returns>
        public String ReadString32()
        {
            var utfLength = ReadInt32();

            if ( utfLength < 0 )
                return null;
            if ( utfLength == 0 )
                return "";

            return doReadString( utfLength );
        }

        /// <summary>
        ///     Method ReadUInt16
        /// </summary>
        /// <returns>An ushort</returns>
        public override UInt16 ReadUInt16() => EndianSupport.SwitchEndian( base.ReadUInt16() );

        /// <summary>
        ///     Method ReadUInt32
        /// </summary>
        /// <returns>An uint</returns>
        public override UInt32 ReadUInt32() => EndianSupport.SwitchEndian( base.ReadUInt32() );

        /// <summary>
        ///     Method ReadUInt64
        /// </summary>
        /// <returns>An ulong</returns>
        public override UInt64 ReadUInt64() => EndianSupport.SwitchEndian( base.ReadUInt64() );

        protected static Exception CreateDataFormatException() => new IOException( "Data format error!" );

        private String doReadString( Int32 utfLength )
        {
            var result = new Char[utfLength];
            var buffer = new Byte[utfLength];

            var bytesRead = 0;
            while ( bytesRead < utfLength )
            {
                var rc = Read( buffer, bytesRead, utfLength - bytesRead );
                if ( rc == 0 )
                    throw new IOException( "premature end of stream" );

                bytesRead += rc;
            }

            var count = 0;
            var index = 0;
            Byte a = 0;

            while ( count < utfLength )
                if ( ( result[index] = (Char) buffer[count++] ) < 0x80 )
                {
                    index++;
                }
                else if ( ( ( a = (Byte) result[index] ) & 0xE0 ) == 0xC0 )
                {
                    if ( count >= utfLength )
                        throw new IOException( "Invalid UTF-8 encoding found, start of two byte char found at end." );

                    var b = buffer[count++];
                    if ( ( b & 0xC0 ) != 0x80 )
                        throw new IOException( "Invalid UTF-8 encoding found, byte two does not start with 0x80." );

                    result[index++] = (Char) ( ( ( a & 0x1F ) << 6 ) | ( b & 0x3F ) );
                }
                else if ( ( a & 0xF0 ) == 0xE0 )
                {
                    if ( count + 1 >= utfLength )
                        throw new IOException( "Invalid UTF-8 encoding found, start of three byte char found at end." );

                    var b = buffer[count++];
                    var c = buffer[count++];
                    if ( ( ( b & 0xC0 ) != 0x80 ) || ( ( c & 0xC0 ) != 0x80 ) )
                        throw new IOException( "Invalid UTF-8 encoding found, byte two does not start with 0x80." );

                    result[index++] = (Char) ( ( ( a & 0x0F ) << 12 ) |
                                               ( ( b & 0x3F ) << 6 ) | ( c & 0x3F ) );
                }
                else
                {
                    throw new IOException( "Invalid UTF-8 encoding found, aborting." );
                }

            return new String( result, 0, index );
        }
    }
}