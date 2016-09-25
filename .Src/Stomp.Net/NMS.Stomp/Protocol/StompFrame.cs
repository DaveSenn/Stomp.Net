// /*
//  * Licensed to the Apache Software Foundation (ASF) under one or more
//  * contributor license agreements.  See the NOTICE file distributed with
//  * this work for additional information regarding copyright ownership.
//  * The ASF licenses this file to You under the Apache License, Version 2.0
//  * (the "License"); you may not use this file except in compliance with
//  * the License.  You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
// 

#region Usings

using System;
using System.Collections;
using System.IO;
using System.Text;

#endregion

namespace Apache.NMS.Stomp.Protocol
{
    public class StompFrame
    {
        #region Constants

        public const Byte BREAK = (Byte) '\n';
        public const Byte COLON = (Byte) ':';
        public const Byte ESCAPE = (Byte) '\\';

        /// Used to mark the End of the Frame.
        public const Byte FRAME_TERMINUS = 0;

        /// Used to denote a Special KeepAlive command that consists of a single newline.
        public const String KEEPALIVE = "KEEPALIVE";

        /// Used to terminate a header line or end of a headers section of the Frame.
        public const String NEWLINE = "\n";

        /// Used to seperate the Key / Value pairing in Frame Headers
        public const String SEPARATOR = ":";

        #endregion

        #region Fields

        public readonly Byte[] COLON_ESCAPE_SEQ = new Byte[2] { 92, 99 };

        private readonly Encoding encoding = new UTF8Encoding();
        public readonly Byte[] ESCAPE_ESCAPE_SEQ = new Byte[2] { 92, 92 };
        public readonly Byte[] NEWLINE_ESCAPE_SEQ = new Byte[2] { 92, 110 };

        #endregion

        #region Properties

        public Boolean EncodingEnabled { get; set; }

        public Byte[] Content { get; set; }

        public String Command { get; set; }

        public IDictionary Properties { get; set; } = new Hashtable();

        #endregion

        #region Ctor

        public StompFrame()
        {
        }

        public StompFrame( Boolean encodingEnabled )
        {
            EncodingEnabled = encodingEnabled;
        }

        public StompFrame( String command )
        {
            Command = command;
        }

        public StompFrame( String command, Boolean encodingEnabled )
        {
            Command = command;
            EncodingEnabled = encodingEnabled;
        }

        #endregion

        public void ClearProperties()
        {
            Properties.Clear();
        }

        public void FromStream( BinaryReader dataIn )
        {
            ReadCommandHeader( dataIn );

            if ( Command != KEEPALIVE )
            {
                ReadHeaders( dataIn );
                ReadContent( dataIn );
            }
        }

        public String GetProperty( String name )
        {
            return GetProperty( name, null );
        }

        public String GetProperty( String name, String fallback )
        {
            if ( Properties.Contains( name ) )
                return Properties[name] as String;

            return fallback;
        }

        public Boolean HasProperty( String name )
        {
            return Properties.Contains( name );
        }

        public String RemoveProperty( String name )
        {
            String result = null;

            if ( Properties.Contains( name ) )
            {
                result = Properties[name] as String;
                Properties.Remove( name );
            }

            return result;
        }

        public void SetProperty( String name, Object value )
        {
            if ( value == null )
                return;

            Properties[name] = value.ToString();
        }

        public void ToStream( BinaryWriter dataOut )
        {
            if ( Command == KEEPALIVE )
            {
                dataOut.Write( BREAK );
                dataOut.Flush();
                return;
            }

            var builder = new StringBuilder();

            builder.Append( Command );
            builder.Append( NEWLINE );

            foreach ( String key in Properties.Keys )
            {
                builder.Append( key );
                builder.Append( SEPARATOR );
                builder.Append( EncodeHeader( Properties[key] as String ) );
                builder.Append( NEWLINE );
            }

            builder.Append( NEWLINE );
            dataOut.Write( encoding.GetBytes( builder.ToString() ) );

            if ( Content != null )
                dataOut.Write( Content );

            dataOut.Write( FRAME_TERMINUS );
        }

        public override String ToString()
        {
            var builder = new StringBuilder();

            builder.Append( GetType()
                                .Name + "[ " );
            builder.Append( "Command=" + Command );
            builder.Append( ", Properties={" );
            foreach ( String key in Properties.Keys )
                builder.Append( " " + key + "=" + Properties[key] );

            builder.Append( "}, " );
            builder.Append( "Content=" + Content ?? Content.ToString() );
            builder.Append( "]" );

            return builder.ToString();
        }

        private String DecodeHeader( String header )
        {
            var decoded = new MemoryStream();

            var value = -1;
            var utf8buf = encoding.GetBytes( header );
            var stream = new MemoryStream( utf8buf );

            while ( ( value = stream.ReadByte() ) != -1 )
                if ( value == 92 )
                {
                    var next = stream.ReadByte();
                    if ( next != -1 )
                        switch ( next )
                        {
                            case 110:
                                decoded.WriteByte( BREAK );
                                break;
                            case 99:
                                decoded.WriteByte( COLON );
                                break;
                            case 92:
                                decoded.WriteByte( ESCAPE );
                                break;
                            default:
                                stream.Seek( -1, SeekOrigin.Current );
                                decoded.WriteByte( (Byte) value );
                                break;
                        }
                    else
                        decoded.WriteByte( (Byte) value );
                }
                else
                {
                    decoded.WriteByte( (Byte) value );
                }

            var data = decoded.ToArray();
            return encoding.GetString( data, 0, data.Length );
        }

        private String EncodeHeader( String header )
        {
            var result = header;
            if ( EncodingEnabled )
            {
                var utf8buf = encoding.GetBytes( header );
                var stream = new MemoryStream( utf8buf.Length );
                foreach ( var val in utf8buf )
                    switch ( val )
                    {
                        case ESCAPE:
                            stream.Write( ESCAPE_ESCAPE_SEQ, 0, ESCAPE_ESCAPE_SEQ.Length );
                            break;
                        case BREAK:
                            stream.Write( NEWLINE_ESCAPE_SEQ, 0, NEWLINE_ESCAPE_SEQ.Length );
                            break;
                        case COLON:
                            stream.Write( COLON_ESCAPE_SEQ, 0, COLON_ESCAPE_SEQ.Length );
                            break;
                        default:
                            stream.WriteByte( val );
                            break;
                    }

                var data = stream.ToArray();
                result = encoding.GetString( data, 0, data.Length );
            }

            return result;
        }

        private void ReadCommandHeader( BinaryReader dataIn )
        {
            Command = ReadLine( dataIn );

            if ( String.IsNullOrEmpty( Command ) )
                Command = "KEEPALIVE";
        }

        private void ReadContent( BinaryReader dataIn )
        {
            if ( Properties.Contains( "content-length" ) )
            {
                var size = Int32.Parse( Properties["content-length"] as String );
                Content = dataIn.ReadBytes( size );

                // Read the terminating NULL byte for this frame.                
                if ( dataIn.Read() != 0 )
                    Tracer.Debug( "StompFrame - Error Invalid Frame, no trailing Null." );
            }
            else
            {
                var ms = new MemoryStream();
                Int32 nextChar;
                while ( ( nextChar = dataIn.ReadByte() ) != 0 )
                {
                    // The first Null in this case marks the end of data.
                    if ( nextChar < 0 )
                        break;

                    ms.WriteByte( (Byte) nextChar );
                }

                Content = ms.ToArray();
            }
        }

        private void ReadHeaders( BinaryReader dataIn )
        {
            String line;
            while ( ( line = ReadLine( dataIn ) ) != "" )
            {
                var idx = line.IndexOf( ':' );

                if ( idx > 0 )
                {
                    var key = line.Substring( 0, idx );
                    var value = line.Substring( idx + 1 );

                    // Stomp v1.1+ allows multiple copies of a property, the first
                    // one is considered to be the newest, we could figure out how
                    // to store them all but for now we just throw the rest out.
                    if ( !Properties.Contains( key ) )
                        Properties[key] = DecodeHeader( value );
                }
                else
                {
                    Tracer.Debug( "StompFrame - Read Malformed Header: " + line );
                }
            }
        }

        private String ReadLine( BinaryReader dataIn )
        {
            var ms = new MemoryStream();

            while ( true )
            {
                var nextChar = dataIn.Read();
                if ( nextChar < 0 )
                    throw new IOException( "Peer closed the stream." );
                if ( nextChar == 10 )
                    break;
                ms.WriteByte( (Byte) nextChar );
            }

            var data = ms.ToArray();
            return encoding.GetString( data, 0, data.Length );
        }
    }
}