#region Usings

using System;
using System.IO;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class StreamMessage : Message, IStreamMessage
    {
        #region Fields

        private MemoryStream byteBuffer;
        private Int32 bytesRemaining = -1;
        private EndianBinaryReader dataIn;
        private EndianBinaryWriter dataOut;

        #endregion

        public override void ClearBody()
        {
            base.ClearBody();
            byteBuffer = null;
            dataIn = null;
            dataOut = null;
            bytesRemaining = -1;
        }

        public Boolean ReadBoolean()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.BOOLEAN_TYPE )
                        return dataIn.ReadBoolean();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Boolean.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a bool" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Boolean type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Byte ReadByte()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.BYTE_TYPE )
                        return dataIn.ReadByte();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Byte.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a byte" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Byte type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int32 ReadBytes( Byte[] value )
        {
            InitializeReading();

            if ( value == null )
                throw new NullReferenceException( "Passed Byte Array is null" );

            try
            {
                if ( bytesRemaining == -1 )
                {
                    var startingPos = byteBuffer.Position;
                    var type = dataIn.ReadByte();

                    if ( type != PrimitiveMap.BYTE_ARRAY_TYPE )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new MessageFormatException( "Not a byte array" );
                    }

                    bytesRemaining = dataIn.ReadInt32();
                }
                else if ( bytesRemaining == 0 )
                {
                    bytesRemaining = -1;
                    return -1;
                }

                if ( value.Length <= bytesRemaining )
                {
                    // small buffer
                    bytesRemaining -= value.Length;
                    dataIn.Read( value, 0, value.Length );
                    return value.Length;
                }
                // big buffer
                var rc = dataIn.Read( value, 0, bytesRemaining );
                bytesRemaining = 0;
                return rc;
            }
            catch ( EndOfStreamException ex )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( ex );
            }
            catch ( IOException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public Char ReadChar()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.CHAR_TYPE )
                        return dataIn.ReadChar();
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a char" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Char type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Double ReadDouble()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.DOUBLE_TYPE )
                        return dataIn.ReadDouble();
                    if ( type == PrimitiveMap.FLOAT_TYPE )
                        return dataIn.ReadSingle();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Single.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a double" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Double type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int16 ReadInt16()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.SHORT_TYPE )
                        return dataIn.ReadInt16();
                    if ( type == PrimitiveMap.BYTE_TYPE )
                        return dataIn.ReadByte();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Int16.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a short" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Int16 type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int32 ReadInt32()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.INTEGER_TYPE )
                        return dataIn.ReadInt32();
                    if ( type == PrimitiveMap.SHORT_TYPE )
                        return dataIn.ReadInt16();
                    if ( type == PrimitiveMap.BYTE_TYPE )
                        return dataIn.ReadByte();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Int32.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a int" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Int32 type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int64 ReadInt64()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.LONG_TYPE )
                        return dataIn.ReadInt64();
                    if ( type == PrimitiveMap.INTEGER_TYPE )
                        return dataIn.ReadInt32();
                    if ( type == PrimitiveMap.SHORT_TYPE )
                        return dataIn.ReadInt16();
                    if ( type == PrimitiveMap.BYTE_TYPE )
                        return dataIn.ReadByte();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Int64.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a long" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Int64 type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Object ReadObject()
        {
            InitializeReading();

            var startingPos = byteBuffer.Position;

            try
            {
                Int32 type = dataIn.ReadByte();

                if ( type == PrimitiveMap.BIG_STRING_TYPE )
                    return dataIn.ReadString32();
                if ( type == PrimitiveMap.STRING_TYPE )
                    return dataIn.ReadString16();
                if ( type == PrimitiveMap.LONG_TYPE )
                    return dataIn.ReadInt64();
                if ( type == PrimitiveMap.INTEGER_TYPE )
                    return dataIn.ReadInt32();
                if ( type == PrimitiveMap.SHORT_TYPE )
                    return dataIn.ReadInt16();
                if ( type == PrimitiveMap.FLOAT_TYPE )
                    return dataIn.ReadSingle();
                if ( type == PrimitiveMap.DOUBLE_TYPE )
                    return dataIn.ReadDouble();
                if ( type == PrimitiveMap.CHAR_TYPE )
                    return dataIn.ReadChar();
                if ( type == PrimitiveMap.BYTE_TYPE )
                    return dataIn.ReadByte();
                if ( type == PrimitiveMap.BOOLEAN_TYPE )
                    return dataIn.ReadBoolean();
                if ( type == PrimitiveMap.BYTE_ARRAY_TYPE )
                {
                    var length = dataIn.ReadInt32();
                    var data = new Byte[length];
                    dataIn.Read( data, 0, length );
                    return data;
                }
                if ( type == PrimitiveMap.NULL )
                    return null;
                byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                throw new MessageFormatException( "Value is not a known type." );
            }
            catch ( FormatException e )
            {
                byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Single ReadSingle()
        {
            InitializeReading();

            try
            {
                var startingPos = byteBuffer.Position;
                try
                {
                    Int32 type = dataIn.ReadByte();

                    if ( type == PrimitiveMap.FLOAT_TYPE )
                        return dataIn.ReadSingle();
                    if ( type == PrimitiveMap.STRING_TYPE )
                        return Single.Parse( dataIn.ReadString16() );
                    if ( type == PrimitiveMap.NULL )
                    {
                        byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                        throw new NMSException( "Cannot convert Null type to a float" );
                    }
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw new MessageFormatException( "Value is not a Single type." );
                }
                catch ( FormatException e )
                {
                    byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                    throw NMSExceptionSupport.CreateMessageFormatException( e );
                }
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public String ReadString()
        {
            InitializeReading();

            var startingPos = byteBuffer.Position;

            try
            {
                Int32 type = dataIn.ReadByte();

                if ( type == PrimitiveMap.BIG_STRING_TYPE )
                    return dataIn.ReadString32();
                if ( type == PrimitiveMap.STRING_TYPE )
                    return dataIn.ReadString16();
                if ( type == PrimitiveMap.LONG_TYPE )
                    return dataIn.ReadInt64()
                                 .ToString();
                if ( type == PrimitiveMap.INTEGER_TYPE )
                    return dataIn.ReadInt32()
                                 .ToString();
                if ( type == PrimitiveMap.SHORT_TYPE )
                    return dataIn.ReadInt16()
                                 .ToString();
                if ( type == PrimitiveMap.FLOAT_TYPE )
                    return dataIn.ReadSingle()
                                 .ToString();
                if ( type == PrimitiveMap.DOUBLE_TYPE )
                    return dataIn.ReadDouble()
                                 .ToString();
                if ( type == PrimitiveMap.CHAR_TYPE )
                    return dataIn.ReadChar()
                                 .ToString();
                if ( type == PrimitiveMap.BYTE_TYPE )
                    return dataIn.ReadByte()
                                 .ToString();
                if ( type == PrimitiveMap.BOOLEAN_TYPE )
                    return dataIn.ReadBoolean()
                                 .ToString();
                if ( type == PrimitiveMap.NULL )
                    return null;
                byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                throw new MessageFormatException( "Value is not a known type." );
            }
            catch ( FormatException e )
            {
                byteBuffer.Seek( startingPos, SeekOrigin.Begin );
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
            catch ( EndOfStreamException e )
            {
                throw NMSExceptionSupport.CreateMessageEOFException( e );
            }
            catch ( IOException e )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public void Reset()
        {
            StoreContent();
            dataIn = null;
            dataOut = null;
            byteBuffer = null;
            bytesRemaining = -1;
            ReadOnlyBody = true;
        }

        public void WriteBoolean( Boolean value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.BOOLEAN_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteByte( Byte value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.BYTE_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteBytes( Byte[] value )
        {
            InitializeWriting();
            WriteBytes( value, 0, value.Length );
        }

        public void WriteBytes( Byte[] value, Int32 offset, Int32 length )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.BYTE_ARRAY_TYPE );
                dataOut.Write( length );
                dataOut.Write( value, offset, length );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteChar( Char value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.CHAR_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteDouble( Double value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.DOUBLE_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt16( Int16 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.SHORT_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt32( Int32 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.INTEGER_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt64( Int64 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.LONG_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteObject( Object value )
        {
            InitializeWriting();
            if ( value is Byte )
                WriteByte( (Byte) value );
            else if ( value is Char )
                WriteChar( (Char) value );
            else if ( value is Boolean )
                WriteBoolean( (Boolean) value );
            else if ( value is Int16 )
                WriteInt16( (Int16) value );
            else if ( value is Int32 )
                WriteInt32( (Int32) value );
            else if ( value is Int64 )
                WriteInt64( (Int64) value );
            else if ( value is Single )
                WriteSingle( (Single) value );
            else if ( value is Double )
                WriteDouble( (Double) value );
            else if ( value is Byte[] )
                WriteBytes( (Byte[]) value );
            else if ( value is String )
                WriteString( (String) value );
            else
                throw new MessageFormatException( "Cannot write non-primitive type:" + value.GetType() );
        }

        public void WriteSingle( Single value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( PrimitiveMap.FLOAT_TYPE );
                dataOut.Write( value );
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public void WriteString( String value )
        {
            InitializeWriting();
            try
            {
                if ( value.Length > 8192 )
                {
                    dataOut.Write( PrimitiveMap.BIG_STRING_TYPE );
                    dataOut.WriteString32( value );
                }
                else
                {
                    dataOut.Write( PrimitiveMap.STRING_TYPE );
                    dataOut.WriteString16( value );
                }
            }
            catch ( IOException e )
            {
                NMSExceptionSupport.Create( e );
            }
        }

        public override Object Clone()
        {
            StoreContent();
            return base.Clone();
        }

        public override Byte GetDataStructureType() => DataStructureTypes.StreamMessageType;

        public override void OnSend()
        {
            base.OnSend();
            StoreContent();
        }

        private void InitializeReading()
        {
            FailIfWriteOnlyBody();
            if ( dataIn == null )
            {
                byteBuffer = new MemoryStream( Content, false );
                dataIn = new EndianBinaryReader( byteBuffer );
            }
        }

        private void InitializeWriting()
        {
            FailIfReadOnlyBody();
            if ( dataOut == null )
            {
                byteBuffer = new MemoryStream();
                dataOut = new EndianBinaryWriter( byteBuffer );
            }
        }

        private void StoreContent()
        {
            if ( dataOut != null )
            {
                dataOut.Close();

                Content = byteBuffer.ToArray();
                dataOut = null;
                byteBuffer = null;
            }
        }
    }
}