#region Usings

using System;
using System.IO;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class BytesMessage : Message, IBytesMessage
    {
        #region Fields

        private EndianBinaryReader _dataIn;
        private EndianBinaryWriter _dataOut;
        private Int32 _length;
        private MemoryStream _outputBuffer;

        #endregion

        public Int64 BodyLength
        {
            get
            {
                InitializeReading();
                return _length;
            }
        }

        public new Byte[] Content
        {
            get
            {
                Byte[] buffer = null;
                InitializeReading();
                if ( _length != 0 )
                {
                    buffer = new Byte[_length];
                    _dataIn.Read( buffer, 0, buffer.Length );
                }
                return buffer;
            }

            set
            {
                InitializeWriting();
                _dataOut.Write( value, 0, value.Length );
            }
        }

        public Boolean ReadBoolean()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadBoolean();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Byte ReadByte()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadByte();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int32 ReadBytes( Byte[] value )
        {
            InitializeReading();
            try
            {
                return _dataIn.Read( value, 0, value.Length );
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int32 ReadBytes( Byte[] value, Int32 length )
        {
            InitializeReading();
            try
            {
                return _dataIn.Read( value, 0, length );
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Char ReadChar()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadChar();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Double ReadDouble()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadDouble();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int16 ReadInt16()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadInt16();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int32 ReadInt32()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadInt32();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Int64 ReadInt64()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadInt64();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public Single ReadSingle()
        {
            InitializeReading();
            try
            {
                return _dataIn.ReadSingle();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public String ReadString()
        {
            InitializeReading();
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                return _dataIn.ReadString16();
            }
            catch ( EndOfStreamException e )
            {
                throw NmsExceptionSupport.CreateMessageEofException( e );
            }
            catch ( IoException e )
            {
                throw NmsExceptionSupport.CreateMessageFormatException( e );
            }
        }

        public void Reset()
        {
            StoreContent();
            _dataIn = null;
            _dataOut = null;
            _outputBuffer = null;
            ReadOnlyBody = true;
        }

        public void WriteBoolean( Boolean value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteByte( Byte value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteBytes( Byte[] value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value, 0, value.Length );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteBytes( Byte[] value, Int32 offset, Int32 length )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value, offset, length );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteChar( Char value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteDouble( Double value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteInt16( Int16 value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteInt32( Int32 value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteInt64( Int64 value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteObject( Object value )
        {
            InitializeWriting();
            if ( value is Byte )
                _dataOut.Write( (Byte) value );
            else if ( value is Char )
                _dataOut.Write( (Char) value );
            else if ( value is Boolean )
                _dataOut.Write( (Boolean) value );
            else if ( value is Int16 )
                _dataOut.Write( (Int16) value );
            else if ( value is Int32 )
                _dataOut.Write( (Int32) value );
            else if ( value is Int64 )
                _dataOut.Write( (Int64) value );
            else if ( value is Single )
                _dataOut.Write( (Single) value );
            else if ( value is Double )
                _dataOut.Write( (Double) value );
            else if ( value is Byte[] )
                _dataOut.Write( (Byte[]) value );
            else if ( value is String )
                _dataOut.WriteString16( (String) value );
            else
                throw new MessageFormatException( "Cannot write non-primitive type:" + value.GetType() );
        }

        public void WriteSingle( Single value )
        {
            InitializeWriting();
            try
            {
                _dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public void WriteString( String value )
        {
            InitializeWriting();
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                _dataOut.WriteString16( value );
            }
            catch ( Exception e )
            {
                throw NmsExceptionSupport.Create( e );
            }
        }

        public override void ClearBody()
        {
            base.ClearBody();
            _outputBuffer = null;
            _dataIn = null;
            _dataOut = null;
            _length = 0;
        }

        public override Object Clone()
        {
            StoreContent();
            return base.Clone();
        }

        public override Byte GetDataStructureType() => DataStructureTypes.BytesMessageType;

        public override void OnSend()
        {
            base.OnSend();
            StoreContent();
        }

        private void InitializeReading()
        {
            FailIfWriteOnlyBody();
            if ( _dataIn == null )
            {
                var data = base.Content;

                if ( base.Content == null )
                    data = new Byte[0];

                Stream target = new MemoryStream( data, false );

                _length = data.Length;
                _dataIn = new EndianBinaryReader( target );
            }
        }

        private void InitializeWriting()
        {
            FailIfReadOnlyBody();
            if ( _dataOut == null )
            {
                _outputBuffer = new MemoryStream();
                _dataOut = new EndianBinaryWriter( _outputBuffer );
            }
        }

        private void StoreContent()
        {
            if ( _dataOut != null )
            {
                _dataOut.Close();
                base.Content = _outputBuffer.ToArray();

                _dataOut = null;
                _outputBuffer = null;
            }
        }
    }
}