#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Extend;

#endregion

namespace Stomp.Net.Stomp.Protocol;

public class StompFrame
{
    public void FromStream( BinaryReader dataIn )
    {
        ReadCommandHeader( dataIn );

        if ( Command == Keepalive )
            return;
        ReadHeaders( dataIn );
        ReadContent( dataIn );
    }

    public Boolean HasProperty( String name )
        => Properties.ContainsKey( name );

    public String RemoveProperty( String name )
    {
        if ( !Properties.ContainsKey( name ) )
            return null;

        var result = Properties[name];
        Properties.Remove( name );

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
        if ( Command == Keepalive )
        {
            dataOut.Write( Break );
            dataOut.Flush();
            return;
        }

        var builder = new StringBuilder();

        builder.Append( Command );
        builder.Append( Newline );

        foreach ( var key in Properties.Keys )
        {
            builder.Append( key );
            builder.Append( Separator );
            builder.Append( EncodeHeader( Properties[key] ) );
            builder.Append( Newline );
        }

        builder.Append( Newline );
        var message = builder.ToString();
        dataOut.Write( _encoding.GetBytes( message ) );

        if ( Content != null )
            dataOut.Write( Content );

        dataOut.Write( FrameTerminus );
    }

    public override String ToString()
    {
        var builder = new StringBuilder();

        builder.Append( GetType()
                            .Name + "[ " );
        builder.Append( "Command=" + Command );
        builder.Append( ", Properties={" );
        foreach ( var key in Properties.Keys )
            builder.Append( " " + key + "=" + Properties[key] );

        builder.Append( "}, " );
        builder.Append( "Content=" + Content );
        builder.Append( "]" );

        return builder.ToString();
    }

    private String DecodeHeader( String header )
    {
        var decoded = new MemoryStream();

        Int32 value;
        var utf8Buf = _encoding.GetBytes( header );
        var stream = new MemoryStream( utf8Buf );

        while ( ( value = stream.ReadByte() ) != -1 )
            if ( value == 92 )
            {
                var next = stream.ReadByte();
                if ( next != -1 )
                    switch ( next )
                    {
                        case 110:
                            decoded.WriteByte( Break );
                            break;
                        case 99:
                            decoded.WriteByte( Colon );
                            break;
                        case 92:
                            decoded.WriteByte( Escape );
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
                decoded.WriteByte( (Byte) value );

        var data = decoded.ToArray();
        return _encoding.GetString( data, 0, data.Length );
    }

    private String EncodeHeader( String header )
    {
        var result = header;
        if ( !EncodingEnabled )
            return result;
        var utf8Buf = _encoding.GetBytes( header );
        var stream = new MemoryStream( utf8Buf.Length );
        foreach ( var val in utf8Buf )
            switch ( val )
            {
                case Escape:
                    stream.Write( _escapeEscapeSeq, 0, _escapeEscapeSeq.Length );
                    break;
                case Break:
                    stream.Write( _newlineEscapeSeq, 0, _newlineEscapeSeq.Length );
                    break;
                case Colon:
                    stream.Write( _colonEscapeSeq, 0, _colonEscapeSeq.Length );
                    break;
                default:
                    stream.WriteByte( val );
                    break;
            }

        var data = stream.ToArray();
        result = _encoding.GetString( data, 0, data.Length );

        return result;
    }

    private void ReadCommandHeader( BinaryReader dataIn )
    {
        Command = ReadLine( dataIn );

        if ( Command.IsEmpty() )
            Command = "KEEPALIVE";
    }

    private void ReadContent( BinaryReader dataIn )
    {
        if ( Properties.ContainsKey( PropertyKeys.ContentLength ) )
        {
            var size = Properties[PropertyKeys.ContentLength]
                .SafeToInt32( Int32.MinValue );
            Content = dataIn.ReadBytes( size );

            // Read the terminating NULL byte for this frame.                
            if ( dataIn.Read() != 0 )
                Tracer.Error( "StompFrame - Error Invalid Frame, no trailing Null." );
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
                if ( !Properties.ContainsKey( key ) )
                    Properties[key] = DecodeHeader( value );
            }
            else if ( Tracer.IsWarnEnabled )
                Tracer.Warn( "StompFrame - Read Malformed Header: " + line );
        }
    }

    private String ReadLine( BinaryReader dataIn )
    {
        var ms = new MemoryStream();

        while ( true )
        {
            var nextChar = dataIn.Read();
            if ( nextChar < 0 )
                throw new IoException( "Peer closed the stream." );

            if ( nextChar == 10 )
                break;

            ms.WriteByte( (Byte) nextChar );
        }

        var data = ms.ToArray();
        return _encoding.GetString( data, 0, data.Length );
    }

    #region Constants

    private const Byte Break = (Byte) '\n';
    private const Byte Colon = (Byte) ':';
    private const Byte Escape = (Byte) '\\';

    /// Used to mark the End of the Frame.
    private const Byte FrameTerminus = 0;

    /// Used to denote a Special KeepAlive command that consists of a single newline.
    public const String Keepalive = "KEEPALIVE";

    /// Used to terminate a header line or end of a headers section of the Frame.
    private const String Newline = "\n";

    /// Used to separate the Key / Value pairing in Frame Headers
    private const String Separator = ":";

    #endregion

    #region Fields

    private readonly Byte[] _colonEscapeSeq = { 92, 99 };
    private readonly Encoding _encoding = Encoding.UTF8;
    private readonly Byte[] _escapeEscapeSeq = { 92, 92 };
    private readonly Byte[] _newlineEscapeSeq = { 92, 110 };

    #endregion

    #region Properties

    private Boolean EncodingEnabled { get; }

    public Byte[] Content { get; set; }

    public String Command { get; set; }

    public Dictionary<String, String> Properties { get; } = new();

    #endregion

    #region Ctor

    public StompFrame( Boolean encodingEnabled ) => EncodingEnabled = encodingEnabled;

    public StompFrame( String command, Boolean encodingEnabled )
    {
        Command = command;
        EncodingEnabled = encodingEnabled;
    }

    #endregion
}