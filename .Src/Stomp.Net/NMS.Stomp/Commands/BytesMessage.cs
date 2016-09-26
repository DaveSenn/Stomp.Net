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

        private EndianBinaryReader dataIn;
        private EndianBinaryWriter dataOut;
        private Int32 length;
        private MemoryStream outputBuffer;

        #endregion

        public Int64 BodyLength
        {
            get
            {
                InitializeReading();
                return length;
            }
        }

        public new Byte[] Content
        {
            get
            {
                Byte[] buffer = null;
                InitializeReading();
                if ( length != 0 )
                {
                    buffer = new Byte[length];
                    dataIn.Read( buffer, 0, buffer.Length );
                }
                return buffer;
            }

            set
            {
                InitializeWriting();
                dataOut.Write( value, 0, value.Length );
            }
        }

        public Boolean ReadBoolean()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadBoolean();
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
                return dataIn.ReadByte();
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
            try
            {
                return dataIn.Read( value, 0, value.Length );
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

        public Int32 ReadBytes( Byte[] value, Int32 length )
        {
            InitializeReading();
            try
            {
                return dataIn.Read( value, 0, length );
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

        public Char ReadChar()
        {
            InitializeReading();
            try
            {
                return dataIn.ReadChar();
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
                return dataIn.ReadDouble();
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
                return dataIn.ReadInt16();
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
                return dataIn.ReadInt32();
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
                return dataIn.ReadInt64();
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
                return dataIn.ReadSingle();
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
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                return dataIn.ReadString16();
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
            outputBuffer = null;
            ReadOnlyBody = true;
        }

        public void WriteBoolean( Boolean value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteByte( Byte value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteBytes( Byte[] value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value, 0, value.Length );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteBytes( Byte[] value, Int32 offset, Int32 length )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value, offset, length );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteChar( Char value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteDouble( Double value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt16( Int16 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt32( Int32 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteInt64( Int64 value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteObject( Object value )
        {
            InitializeWriting();
            if ( value is Byte )
                dataOut.Write( (Byte) value );
            else if ( value is Char )
                dataOut.Write( (Char) value );
            else if ( value is Boolean )
                dataOut.Write( (Boolean) value );
            else if ( value is Int16 )
                dataOut.Write( (Int16) value );
            else if ( value is Int32 )
                dataOut.Write( (Int32) value );
            else if ( value is Int64 )
                dataOut.Write( (Int64) value );
            else if ( value is Single )
                dataOut.Write( (Single) value );
            else if ( value is Double )
                dataOut.Write( (Double) value );
            else if ( value is Byte[] )
                dataOut.Write( (Byte[]) value );
            else if ( value is String )
                dataOut.WriteString16( (String) value );
            else
                throw new MessageFormatException( "Cannot write non-primitive type:" + value.GetType() );
        }

        public void WriteSingle( Single value )
        {
            InitializeWriting();
            try
            {
                dataOut.Write( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public void WriteString( String value )
        {
            InitializeWriting();
            try
            {
                // JMS, CMS and NMS all encode the String using a 16 bit size header.
                dataOut.WriteString16( value );
            }
            catch ( Exception e )
            {
                throw NMSExceptionSupport.Create( e );
            }
        }

        public override void ClearBody()
        {
            base.ClearBody();
            outputBuffer = null;
            dataIn = null;
            dataOut = null;
            length = 0;
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
            if ( dataIn == null )
            {
                var data = base.Content;

                if ( base.Content == null )
                    data = new Byte[0];

                Stream target = new MemoryStream( data, false );

                length = data.Length;
                dataIn = new EndianBinaryReader( target );
            }
        }

        private void InitializeWriting()
        {
            FailIfReadOnlyBody();
            if ( dataOut == null )
            {
                outputBuffer = new MemoryStream();
                dataOut = new EndianBinaryWriter( outputBuffer );
            }
        }

        private void StoreContent()
        {
            if ( dataOut != null )
            {
                dataOut.Close();
                base.Content = outputBuffer.ToArray();

                dataOut = null;
                outputBuffer = null;
            }
        }
    }
}