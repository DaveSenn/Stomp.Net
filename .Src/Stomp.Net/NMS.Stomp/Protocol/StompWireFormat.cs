#region Usings

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Apache.NMS.Stomp.Commands;
using Apache.NMS.Stomp.Transport;
using Apache.NMS.Util;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Protocol
{
    /// <summary>
    ///     Implements the <a href="http://stomp.codehaus.org/">STOMP</a> protocol.
    /// </summary>
    public class StompWireFormat : IWireFormat
    {
        #region Fields

        private Int32 connectedResponseId = -1;
        private Boolean encodeHeaders;
        private WireFormatInfo remoteWireFormatInfo;

        #endregion

        #region Properties

        public Int32 Version => 1;

        public Encoding Encoder { get; set; } = new UTF8Encoding();

        public IPrimitiveMapMarshaler MapMarshaler { get; set; } = new XmlPrimitiveMapMarshaler();

        public Int32 MaxInactivityDuration { get; set; } = 30000;

        public Int32 MaxInactivityDurationInitialDelay { get; set; } = 0;

        public Int64 ReadCheckInterval => MaxInactivityDuration;

        public Int64 WriteCheckInterval => MaxInactivityDuration > 3 ? MaxInactivityDuration / 3 : MaxInactivityDuration;

        #endregion

        public void Marshal( Object o, BinaryWriter writer )
        {
            if ( o is ConnectionInfo )
            {
                WriteConnectionInfo( (ConnectionInfo) o, writer );
            }
            else if ( o is Message )
            {
                WriteMessage( (Message) o, writer );
            }
            else if ( o is ConsumerInfo )
            {
                WriteConsumerInfo( (ConsumerInfo) o, writer );
            }
            else if ( o is MessageAck )
            {
                WriteMessageAck( (MessageAck) o, writer );
            }
            else if ( o is TransactionInfo )
            {
                WriteTransactionInfo( (TransactionInfo) o, writer );
            }
            else if ( o is ShutdownInfo )
            {
                WriteShutdownInfo( (ShutdownInfo) o, writer );
            }
            else if ( o is RemoveInfo )
            {
                WriteRemoveInfo( (RemoveInfo) o, writer );
            }
            else if ( o is KeepAliveInfo )
            {
                WriteKeepAliveInfo( (KeepAliveInfo) o, writer );
            }
            else if ( o is ICommand )
            {
                var command = o as ICommand;
                if ( command.ResponseRequired )
                {
                    var response = new Response { CorrelationId = command.CommandId };
                    SendCommand( response );
                }
            }
            else
            {
                Tracer.Warn( $"StompWireFormat - Ignored command: {o.GetType()} => '{0}'" );
            }
        }

        public ITransport Transport { get; set; }

        public ICommand Unmarshal( BinaryReader reader )
        {
            var frame = new StompFrame( encodeHeaders );
            frame.FromStream( reader );

            var answer = CreateCommand( frame );
            return answer;
        }

        protected virtual ICommand CreateCommand( StompFrame frame )
        {
            var command = frame.Command;

            if ( command == "RECEIPT" )
            {
                var text = frame.RemoveProperty( "receipt-id" );
                if ( text != null )
                {
                    var answer = new Response();
                    if ( text.StartsWith( "ignore:" ) )
                        text = text.Substring( "ignore:".Length );

                    answer.CorrelationId = Int32.Parse( text );
                    return answer;
                }
            }
            else if ( command == "CONNECTED" )
            {
                return ReadConnected( frame );
            }
            else if ( command == "ERROR" )
            {
                var text = frame.RemoveProperty( "receipt-id" );

                if ( text != null && text.StartsWith( "ignore:" ) )
                {
                    var answer = new Response();
                    answer.CorrelationId = Int32.Parse( text.Substring( "ignore:".Length ) );
                    return answer;
                }
                else
                {
                    var answer = new ExceptionResponse();
                    if ( text != null )
                        answer.CorrelationId = Int32.Parse( text );

                    var error = new BrokerError();
                    error.Message = frame.RemoveProperty( "message" );
                    answer.Exception = error;
                    return answer;
                }
            }
            else if ( command == "KEEPALIVE" )
            {
                return new KeepAliveInfo();
            }
            else if ( command == "MESSAGE" )
            {
                return ReadMessage( frame );
            }

            Tracer.Error( "Unknown command: " + frame.Command + " headers: " + frame.Properties );

            return null;
        }

        protected virtual ICommand ReadConnected( StompFrame frame )
        {
            remoteWireFormatInfo = new WireFormatInfo();

            if ( frame.HasProperty( "version" ) )
            {
                remoteWireFormatInfo.Version = Single.Parse( frame.RemoveProperty( "version" ),
                                                             CultureInfo.InvariantCulture );
                if ( remoteWireFormatInfo.Version > 1.0f )
                    encodeHeaders = true;

                if ( frame.HasProperty( "session" ) )
                    remoteWireFormatInfo.Session = frame.RemoveProperty( "session" );

                if ( frame.HasProperty( "heart-beat" ) )
                {
                    var hearBeats = frame.RemoveProperty( "heart-beat" )
                                         .Split( ",".ToCharArray() );
                    if ( hearBeats.Length != 2 )
                        throw new IoException( "Malformed heartbeat property in Connected Frame." );

                    remoteWireFormatInfo.WriteCheckInterval = Int32.Parse( hearBeats[0].Trim() );
                    remoteWireFormatInfo.ReadCheckInterval = Int32.Parse( hearBeats[1].Trim() );
                }
            }
            else
            {
                remoteWireFormatInfo.ReadCheckInterval = 0;
                remoteWireFormatInfo.WriteCheckInterval = 0;
                remoteWireFormatInfo.Version = 1.0f;
            }

            if ( connectedResponseId != -1 )
            {
                var answer = new Response();
                answer.CorrelationId = connectedResponseId;
                SendCommand( answer );
                connectedResponseId = -1;
            }
            else
            {
                throw new IoException( "Received Connected Frame without a set Response Id for it." );
            }

            return remoteWireFormatInfo;
        }

        protected virtual ICommand ReadMessage( StompFrame frame )
        {
            Message message = null;
            var transformation = frame.RemoveProperty( "transformation" );

            if ( frame.HasProperty( "content-length" ) )
            {
                message = new BytesMessage();
                message.Content = frame.Content;
            }
            else if ( transformation == "jms-map-xml" )
            {
                message = new MapMessage( MapMarshaler.Unmarshal( frame.Content ) as PrimitiveMap );
            }
            else
            {
                message = new TextMessage( Encoder.GetString( frame.Content, 0, frame.Content.Length ) );
            }

            // Remove any receipt header we might have attached if the outbound command was
            // sent with response required set to true
            frame.RemoveProperty( "receipt" );

            // Clear any attached content length headers as they aren't needed anymore and can
            // clutter the Message Properties.
            frame.RemoveProperty( "content-length" );

            message.Type = frame.RemoveProperty( "type" );
            message.Destination = Destination.ConvertToDestination( frame.RemoveProperty( "destination" ) );
            message.ReplyTo = Destination.ConvertToDestination( frame.RemoveProperty( "reply-to" ) );
            message.TargetConsumerId = new ConsumerId( frame.RemoveProperty( "subscription" ) );
            message.CorrelationId = frame.RemoveProperty( "correlation-id" );
            message.MessageId = new MessageId( frame.RemoveProperty( "message-id" ) );
            message.Persistent = StompHelper.ToBool( frame.RemoveProperty( "persistent" ), false );

            // If it came from NMS.Stomp we added this header to ensure its reported on the
            // receiver side.
            if ( frame.HasProperty( "NMSXDeliveryMode" ) )
                message.Persistent = StompHelper.ToBool( frame.RemoveProperty( "NMSXDeliveryMode" ), false );

            if ( frame.HasProperty( "priority" ) )
                message.Priority = Byte.Parse( frame.RemoveProperty( "priority" ) );

            if ( frame.HasProperty( "timestamp" ) )
                message.Timestamp = Int64.Parse( frame.RemoveProperty( "timestamp" ) );

            if ( frame.HasProperty( "expires" ) )
                message.Expiration = Int64.Parse( frame.RemoveProperty( "expires" ) );

            if ( frame.RemoveProperty( "redelivered" ) != null )
                message.RedeliveryCounter = 1;

            // now lets add the generic headers
            foreach ( String key in frame.Properties.Keys )
            {
                var value = frame.Properties[key];
                if ( value != null )
                    if ( key == "JMSXGroupSeq" || key == "NMSXGroupSeq" )
                    {
                        value = Int32.Parse( value.ToString() );
                        message.Properties["NMSXGroupSeq"] = value;
                        continue;
                    }
                    else if ( key == "JMSXGroupID" || key == "NMSXGroupID" )
                    {
                        message.Properties["NMSXGroupID"] = value;
                        continue;
                    }
                message.Properties[key] = value;
            }

            var dispatch = new MessageDispatch();
            dispatch.Message = message;
            dispatch.ConsumerId = message.TargetConsumerId;
            dispatch.Destination = message.Destination;
            dispatch.RedeliveryCounter = message.RedeliveryCounter;

            return dispatch;
        }

        protected virtual void SendCommand( ICommand command )
        {
            if ( Transport == null )
                Tracer.Fatal( "No transport configured so cannot return command: " + command );
            else
                Transport.Command( Transport, command );
        }

        protected virtual String ToString( Object value )
        {
            if ( value != null )
                return value.ToString();
            return null;
        }

        protected virtual void WriteConnectionInfo( ConnectionInfo command, BinaryWriter dataOut )
        {
            // lets force a receipt for the Connect Frame.
            var frame = new StompFrame( "CONNECT", encodeHeaders );

            frame.SetProperty( "client-id", command.ClientId );
            if ( !String.IsNullOrEmpty( command.UserName ) )
                frame.SetProperty( "login", command.UserName );
            if ( !String.IsNullOrEmpty( command.Password ) )
                frame.SetProperty( "passcode", command.Password );
            frame.SetProperty( "host", command.Host );
            frame.SetProperty( "accept-version", "1.0,1.1" );

            if ( MaxInactivityDuration != 0 )
                frame.SetProperty( "heart-beat", WriteCheckInterval + "," + ReadCheckInterval );

            connectedResponseId = command.CommandId;

            frame.ToStream( dataOut );
        }

        protected virtual void WriteConsumerInfo( ConsumerInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "SUBSCRIBE", encodeHeaders );

            if ( command.ResponseRequired )
                frame.SetProperty( "receipt", command.CommandId );

            frame.SetProperty( "destination", Destination.ConvertToStompString( command.Destination ) );
            frame.SetProperty( "id", command.ConsumerId.ToString() );
            frame.SetProperty( "durable-subscriber-name", command.SubscriptionName );
            frame.SetProperty( "selector", command.Selector );
            frame.SetProperty( "ack", StompHelper.ToStomp( command.AckMode ) );

            if ( command.NoLocal )
                frame.SetProperty( "no-local", command.NoLocal.ToString() );

            // ActiveMQ extensions to STOMP

            if ( command.Transformation != null )
                frame.SetProperty( "transformation", command.Transformation );
            else
                frame.SetProperty( "transformation", "jms-xml" );

            frame.SetProperty( "activemq.dispatchAsync", command.DispatchAsync );

            if ( command.Exclusive )
                frame.SetProperty( "activemq.exclusive", command.Exclusive );

            if ( command.SubscriptionName != null )
            {
                frame.SetProperty( "activemq.subscriptionName", command.SubscriptionName );
                // For an older 4.0 broker we need to set this header so they get the
                // subscription as well..
                frame.SetProperty( "activemq.subcriptionName", command.SubscriptionName );
            }

            frame.SetProperty( "activemq.maximumPendingMessageLimit", command.MaximumPendingMessageLimit );
            frame.SetProperty( "activemq.prefetchSize", command.PrefetchSize );
            frame.SetProperty( "activemq.priority", command.Priority );

            if ( command.Retroactive )
                frame.SetProperty( "activemq.retroactive", command.Retroactive );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteKeepAliveInfo( KeepAliveInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( StompFrame.KEEPALIVE, encodeHeaders );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteMessage( Message command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "SEND", encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( "receipt", command.CommandId );

            frame.SetProperty( "destination", Destination.ConvertToStompString( command.Destination ) );

            if ( command.ReplyTo != null )
                frame.SetProperty( "reply-to", Destination.ConvertToStompString( command.ReplyTo ) );
            if ( command.CorrelationId != null )
                frame.SetProperty( "correlation-id", command.CorrelationId );
            if ( command.Expiration != 0 )
                frame.SetProperty( "expires", command.Expiration );
            if ( command.Timestamp != 0 )
                frame.SetProperty( "timestamp", command.Timestamp );
            if ( command.Priority != 4 )
                frame.SetProperty( "priority", command.Priority );
            if ( command.Type != null )
                frame.SetProperty( "type", command.Type );
            if ( command.TransactionId != null )
                frame.SetProperty( "transaction", command.TransactionId.ToString() );

            frame.SetProperty( "persistent",
                               command.Persistent.ToString()
                                      .ToLower() );
            frame.SetProperty( "NMSXDeliveryMode",
                               command.Persistent.ToString()
                                      .ToLower() );

            if ( command.NMSXGroupID != null )
            {
                frame.SetProperty( "JMSXGroupID", command.NMSXGroupID );
                frame.SetProperty( "NMSXGroupID", command.NMSXGroupID );
                frame.SetProperty( "JMSXGroupSeq", command.NMSXGroupSeq );
                frame.SetProperty( "NMSXGroupSeq", command.NMSXGroupSeq );
            }

            // Perform any Content Marshaling.
            command.BeforeMarshall( this );

            // Store the Marshaled Content.
            frame.Content = command.Content;

            if ( command is BytesMessage )
            {
                if ( command.Content != null && command.Content.Length > 0 )
                    frame.SetProperty( "content-length", command.Content.Length );

                frame.SetProperty( "transformation", "jms-byte" );
            }
            else if ( command is MapMessage )
            {
                frame.SetProperty( "transformation", MapMarshaler.Name );
            }

            // Marshal all properties to the Frame.
            var map = command.Properties;
            foreach ( String key in map.Keys )
                frame.SetProperty( key, map[key] );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteMessageAck( MessageAck command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "ACK", encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( "receipt", "ignore:" + command.CommandId );

            frame.SetProperty( "message-id", command.LastMessageId.ToString() );
            frame.SetProperty( "subscription", command.ConsumerId.ToString() );

            if ( command.TransactionId != null )
                frame.SetProperty( "transaction", command.TransactionId.ToString() );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteRemoveInfo( RemoveInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "UNSUBSCRIBE", encodeHeaders );
            Object id = command.ObjectId;

            if ( id is ConsumerId )
            {
                var consumerId = id as ConsumerId;
                if ( command.ResponseRequired )
                    frame.SetProperty( "receipt", command.CommandId );
                frame.SetProperty( "id", consumerId.ToString() );

                frame.ToStream( dataOut );
            }
        }

        protected virtual void WriteShutdownInfo( ShutdownInfo command, BinaryWriter dataOut )
        {
            Debug.Assert( !command.ResponseRequired );

            var frame = new StompFrame( "DISCONNECT", encodeHeaders );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteTransactionInfo( TransactionInfo command, BinaryWriter dataOut )
        {
            String type;
            switch ( (TransactionType) command.Type )
            {
                case TransactionType.Commit:
                    command.ResponseRequired = true;
                    type = "COMMIT";
                    break;
                case TransactionType.Rollback:
                    command.ResponseRequired = true;
                    type = "ABORT";
                    break;
                case TransactionType.Begin:
                    type = "BEGIN";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var frame = new StompFrame( type, encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( "receipt", command.CommandId );

            frame.SetProperty( "transaction", command.TransactionId.ToString() );

            frame.ToStream( dataOut );
        }
    }
}