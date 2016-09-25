

#region Usings

using System;
using System.IO;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     A BinaryWriter that switches the endian orientation of the write operations so that they
    ///     are compatible across platforms.
    /// </summary>
    [CLSCompliant( false )]
    public class EndianBinaryWriter : BinaryWriter
    {
        #region Constants

        public const Int32 MAXSTRINGLEN = Int16.MaxValue;

        #endregion

        #region Ctor

        public EndianBinaryWriter( Stream output )
            : base( output )
        {
        }

        #endregion

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">A  long</param>
        public override void Write( Int64 value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">An ushort</param>
        public override void Write( UInt16 value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">An int</param>
        public override void Write( Int32 value )
        {
            var x = EndianSupport.SwitchEndian( value );
            base.Write( x );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="chars">A  char[]</param>
        /// <param name="index">An int</param>
        /// <param name="count">An int</param>
        public override void Write( Char[] chars, Int32 index, Int32 count )
        {
            var t = new Char[count];
            for ( var i = 0; i < count; i++ )
                t[index + i] = EndianSupport.SwitchEndian( t[index + i] );
            base.Write( t );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="chars">A  char[]</param>
        public override void Write( Char[] chars )
        {
            Write( chars, 0, chars.Length );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">An uint</param>
        public override void Write( UInt32 value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="ch">A  char</param>
        public override void Write( Char ch )
        {
            base.Write( (Byte) ( ( ch >> 8 ) & 0xFF ) );
            base.Write( (Byte) ( ch & 0xFF ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">An ulong</param>
        public override void Write( UInt64 value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">A  short</param>
        public override void Write( Int16 value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write, writes a string to the output using the WriteString16
        ///     method.
        /// </summary>
        /// <param name="text">A  string</param>
        public override void Write( String text )
        {
            WriteString16( text );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">A  double</param>
        public override void Write( Single value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method Write
        /// </summary>
        /// <param name="value">A  double</param>
        public override void Write( Double value )
        {
            base.Write( EndianSupport.SwitchEndian( value ) );
        }

        /// <summary>
        ///     Method WriteString16, writes a string to the output using the Java
        ///     standard modified UTF-8 encoding with an unsigned short value written first to
        ///     indicate the length of the encoded data, the short is read as an unsigned
        ///     value so the max amount of data this method can write is 65535 encoded bytes.
        ///     Unlike the WriteString32 method this method does not encode the length
        ///     value to -1 if the string is null, this is to match the behaviour of
        ///     the Java DataOuputStream class's writeUTF method.
        ///     Because modified UTF-8 encding can result in a number of bytes greater that
        ///     the size of the String this method must first check that the encoding proces
        ///     will not result in a value that cannot be written becuase it is greater than
        ///     the max value of an unsigned short.
        /// </summary>
        /// <param name="text">A  string</param>
        public void WriteString16( String text )
        {
            if ( text != null )
            {
                if ( text.Length > UInt16.MaxValue )
                    throw new IOException(
                        String.Format(
                            "Cannot marshall string longer than: {0} characters, supplied string was: " +
                            "{1} characters",
                            UInt16.MaxValue,
                            text.Length ) );

                var charr = text.ToCharArray();
                var utfLength = CountUtf8Bytes( charr );

                if ( utfLength > UInt16.MaxValue )
                    throw new IOException(
                        String.Format(
                            "Cannot marshall an encoded string longer than: {0} bytes, supplied" +
                            "string requires: {1} characters to encode",
                            UInt16.MaxValue,
                            utfLength ) );

                var bytearr = new Byte[utfLength];
                encodeUTF8toBuffer( charr, bytearr );

                Write( (UInt16) utfLength );
                Write( bytearr );
            }
        }

        /// <summary>
        ///     Method WriteString32, writes a string to the output using the Openwire
        ///     standard modified UTF-8 encoding which an int value written first to
        ///     indicate the length of the encoded data, the int is read as an signed
        ///     value so the max amount of data this method can write is 2^31 encoded bytes.
        ///     In the case of a null value being passed this method writes a -1 to the
        ///     stream to indicate that the string is null.
        ///     Because modified UTF-8 encding can result in a number of bytes greater that
        ///     the size of the String this method must first check that the encoding proces
        ///     will not result in a value that cannot be written becuase it is greater than
        ///     the max value of an int.
        /// </summary>
        /// <param name="text">A  string</param>
        public void WriteString32( String text )
        {
            if ( text != null )
            {
                var charr = text.ToCharArray();
                var utfLength = CountUtf8Bytes( charr );

                if ( utfLength > Int32.MaxValue )
                    throw new IOException(
                        String.Format(
                            "Cannot marshall an encoded string longer than: {0} bytes, supplied" +
                            "string requires: {1} characters to encode",
                            Int32.MaxValue,
                            utfLength ) );

                var bytearr = new Byte[utfLength];
                encodeUTF8toBuffer( charr, bytearr );

                Write( utfLength );
                Write( bytearr );
            }
            else
            {
                Write( -1 );
            }
        }

        private static UInt32 CountUtf8Bytes( Char[] chars )
        {
            UInt32 utfLength = 0;
            var c = 0;

            for ( var i = 0; i < chars.Length; i++ )
            {
                c = chars[i];
                if ( ( c >= 0x0001 ) && ( c <= 0x007F ) )
                    utfLength++;
                else if ( c > 0x07FF )
                    utfLength += 3;
                else
                    utfLength += 2;
            }

            return utfLength;
        }

        private static void encodeUTF8toBuffer( Char[] chars, Byte[] buffer )
        {
            var c = 0;
            var count = 0;

            for ( var i = 0; i < chars.Length; i++ )
            {
                c = chars[i];
                if ( ( c >= 0x0001 ) && ( c <= 0x007F ) )
                {
                    buffer[count++] = (Byte) c;
                }
                else if ( c > 0x07FF )
                {
                    buffer[count++] = (Byte) ( 0xE0 | ( ( c >> 12 ) & 0x0F ) );
                    buffer[count++] = (Byte) ( 0x80 | ( ( c >> 6 ) & 0x3F ) );
                    buffer[count++] = (Byte) ( 0x80 | ( ( c >> 0 ) & 0x3F ) );
                }
                else
                {
                    buffer[count++] = (Byte) ( 0xC0 | ( ( c >> 6 ) & 0x1F ) );
                    buffer[count++] = (Byte) ( 0x80 | ( ( c >> 0 ) & 0x3F ) );
                }
            }
        }
    }
}