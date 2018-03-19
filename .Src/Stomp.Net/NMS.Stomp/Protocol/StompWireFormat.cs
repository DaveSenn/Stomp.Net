#region Usings

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Extend;
using Stomp.Net.Stomp.Commands;
using Stomp.Net.Stomp.Transport;

#endregion

namespace Stomp.Net.Stomp.Protocol
{
    /// <summary>
    ///     Implements the STOMP protocol.
    /// </summary>
    public class StompWireFormat : IWireFormat
    {
        #region Fields

        private Int32 _connectedResponseId = -1;
        private Boolean _encodeHeaders;
        private WireFormatInfo _remoteWireFormatInfo;

        #endregion

        #region Properties

        public Int32 MaxInactivityDuration { get; } = 30000;

        public Int32 MaxInactivityDurationInitialDelay { get; } = 0;

        public Int32 ReadCheckInterval => MaxInactivityDuration;

        public Int32 WriteCheckInterval => MaxInactivityDuration > 3 ? MaxInactivityDuration / 3 : MaxInactivityDuration;

        public ITransport Transport { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the destination name formatting should be skipped or not.
        ///     If set to true the physical name property will be used as stomp destination string without adding prefixes such as
        ///     queue or topic. This to support JMS brokers listening for queue/topic names in a different format.
        /// </summary>
        public Boolean SkipDesinationNameFormatting { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the host header will be set or not.
        /// </summary>
        /// <remarks>
        ///     Disabling the host header can make sens if you are working with a broker like RabbitMq
        ///     which uses the host header as name of the target virtual host.
        /// </remarks>
        public Boolean SetHostHeader { get; set; }

        /// <summary>
        ///     Gets or sets the value used as host header.
        ///     If set Stomp.Net will use this value as content of the host header.
        /// </summary>
        public String HostHeaderOverride { get; set; }

        #endregion

        public void Marshal( Object o, BinaryWriter writer )
        {
            switch ( o )
            {
                case ConnectionInfo info:
                    WriteConnectionInfo( info, writer );
                    break;
                case BytesMessage _:
                    WriteMessage( (BytesMessage) o, writer );
                    break;
                case ConsumerInfo _:
                    WriteConsumerInfo( (ConsumerInfo) o, writer );
                    break;
                case MessageAck _:
                    WriteMessageAck( (MessageAck) o, writer );
                    break;
                case TransactionInfo _:
                    WriteTransactionInfo( (TransactionInfo) o, writer );
                    break;
                case ShutdownInfo _:
                    WriteShutdownInfo( (ShutdownInfo) o, writer );
                    break;
                case RemoveInfo _:
                    WriteRemoveInfo( (RemoveInfo) o, writer );
                    break;
                case KeepAliveInfo _:
                    WriteKeepAliveInfo( (KeepAliveInfo) o, writer );
                    break;
                case ICommand command:
                    if ( !command.ResponseRequired )
                        return;
                    var response = new Response { CorrelationId = command.CommandId };
                    SendCommand( response );
                    break;
                default:
                    Tracer.Warn( $"StompWireFormat - Ignored command: {o.GetType()} => '{0}'" );
                    break;
            }
        }

        public ICommand Unmarshal( BinaryReader reader )
        {
            var frame = new StompFrame( _encodeHeaders );
            frame.FromStream( reader );

            var answer = CreateCommand( frame );
            return answer;
        }

        protected virtual ICommand CreateCommand( StompFrame frame )
        {
            var command = frame.Command;

            switch ( command )
            {
                case "RECEIPT":
                {
                    var text = frame.RemoveProperty( PropertyKeys.ReceiptId );
                    if ( text != null )
                    {
                        var answer = new Response();
                        if ( text.StartsWith( "ignore:", StringComparison.Ordinal ) )
                            text = text.Substring( "ignore:".Length );

                        answer.CorrelationId = Int32.Parse( text );
                        return answer;
                    }
                }
                    break;
                case "CONNECTED":
                    return ReadConnected( frame );
                case "ERROR":
                {
                    var text = frame.RemoveProperty( PropertyKeys.ReceiptId );

                    if ( text != null && text.StartsWith( "ignore:", StringComparison.Ordinal ) )
                        return new Response { CorrelationId = Int32.Parse( text.Substring( "ignore:".Length ) ) };

                    var answer = new ExceptionResponse();
                    if ( text != null )
                        answer.CorrelationId = Int32.Parse( text );

                    var error = new BrokerError { Message = frame.RemoveProperty( PropertyKeys.Message ) };
                    answer.Exception = error;
                    return answer;
                }
                case "KEEPALIVE":
                    return new KeepAliveInfo();
                case "MESSAGE":
                    return ReadMessage( frame );
            }

            Tracer.Error( "Unknown command: " + frame.Command + " headers: " + frame.Properties );

            return null;
        }

        protected virtual ICommand ReadConnected( StompFrame frame )
        {
            _remoteWireFormatInfo = new WireFormatInfo();

            if ( frame.HasProperty( PropertyKeys.Version ) )
            {
                _remoteWireFormatInfo.Version = Single.Parse( frame.RemoveProperty( PropertyKeys.Version ), CultureInfo.InvariantCulture );
                if ( _remoteWireFormatInfo.Version > 1.0f )
                    _encodeHeaders = true;

                if ( frame.HasProperty( PropertyKeys.Session ) )
                    _remoteWireFormatInfo.Session = frame.RemoveProperty( PropertyKeys.Session );

                if ( frame.HasProperty( PropertyKeys.HartBeat ) )
                {
                    var hearBeats = frame.RemoveProperty( PropertyKeys.HartBeat )
                                         .Split( ",".ToCharArray() );
                    if ( hearBeats.Length != 2 )
                        throw new IoException( "Malformed heartbeat property in Connected Frame." );

                    _remoteWireFormatInfo.WriteCheckInterval = Int32.Parse( hearBeats[0]
                                                                                .Trim() );
                    _remoteWireFormatInfo.ReadCheckInterval = Int32.Parse( hearBeats[1]
                                                                               .Trim() );
                }
            }
            else
            {
                _remoteWireFormatInfo.ReadCheckInterval = 0;
                _remoteWireFormatInfo.WriteCheckInterval = 0;
                _remoteWireFormatInfo.Version = 1.0f;
            }

            if ( _connectedResponseId != -1 )
            {
                var answer = new Response { CorrelationId = _connectedResponseId };
                SendCommand( answer );
                _connectedResponseId = -1;
            }
            else
                throw new IoException( "Received Connected Frame without a set Response Id for it." );

            return _remoteWireFormatInfo;
        }

        protected virtual ICommand ReadMessage( StompFrame frame )
        {
            frame.RemoveProperty( PropertyKeys.Transformation );

            var message = new BytesMessage { Content = frame.Content };

            // Remove any receipt header we might have attached if the outbound command was
            // sent with response required set to true
            frame.RemoveProperty( PropertyKeys.Receipt );

            // Clear any attached content length headers as they aren't needed anymore and can
            // clutter the Message Properties.
            frame.RemoveProperty( PropertyKeys.ContentLength );

            message.Type = frame.RemoveProperty( PropertyKeys.Type );
            message.Destination = Destination.ConvertToDestination( frame.RemoveProperty( PropertyKeys.Destination ), SkipDesinationNameFormatting );
            message.ReplyTo = Destination.ConvertToDestination( frame.RemoveProperty( PropertyKeys.ReplyTo ), SkipDesinationNameFormatting );
            message.TargetConsumerId = new ConsumerId( frame.RemoveProperty( PropertyKeys.Subscription ) );
            message.CorrelationId = frame.RemoveProperty( PropertyKeys.CorrelationId );
            message.MessageId = new MessageId( frame.RemoveProperty( PropertyKeys.MessageId ) );
            message.Persistent = StompHelper.ToBool( frame.RemoveProperty( PropertyKeys.Persistent ), false );

            // If it came from NMS.Stomp we added this header to ensure its reported on the
            // receiver side.
            if ( frame.HasProperty( PropertyKeys.NmsxDeliveryMode ) )
                message.Persistent = StompHelper.ToBool( frame.RemoveProperty( PropertyKeys.NmsxDeliveryMode ), false );

            if ( frame.HasProperty( PropertyKeys.Priority ) )
                message.Priority = Byte.Parse( frame.RemoveProperty( PropertyKeys.Priority ) );

            if ( frame.HasProperty( PropertyKeys.TimeStamp ) )
                message.Timestamp = Int64.Parse( frame.RemoveProperty( PropertyKeys.TimeStamp ) );

            if ( frame.HasProperty( PropertyKeys.Expires ) )
                message.Expiration = Int64.Parse( frame.RemoveProperty( PropertyKeys.Expires ) );

            if ( frame.RemoveProperty( PropertyKeys.Redelivered ) != null )
                message.RedeliveryCounter = 1;

            // now lets add the generic headers
            foreach ( var key in frame.Properties.Keys )
            {
                var value = frame.Properties[key];
                message.Headers[key] = value;
            }

            return new MessageDispatch( message.TargetConsumerId, message.Destination, message, message.RedeliveryCounter );
        }

        protected virtual void SendCommand( ICommand command )
        {
            if ( Transport == null )
                Tracer.Fatal( "No transport configured so cannot return command: " + command );
            else
                Transport.Command( Transport, command );
        }

        protected virtual void WriteConnectionInfo( ConnectionInfo command, BinaryWriter dataOut )
        {
            // lets force a receipt for the Connect Frame.
            var frame = new StompFrame( "CONNECT", _encodeHeaders );

            frame.SetProperty( PropertyKeys.ClientId, command.ClientId );
            if ( command.UserName.IsNotEmpty() )
                frame.SetProperty( PropertyKeys.Login, command.UserName );
            if ( command.Password.IsNotEmpty() )
                frame.SetProperty( PropertyKeys.Passcode, command.Password );

            if ( SetHostHeader )
                frame.SetProperty( PropertyKeys.Host, HostHeaderOverride ?? command.Host );

            frame.SetProperty( PropertyKeys.AcceptVersion, "1.0,1.1" );

            if ( MaxInactivityDuration != 0 )
                frame.SetProperty( PropertyKeys.HartBeat, WriteCheckInterval + "," + ReadCheckInterval );

            _connectedResponseId = command.CommandId;

            frame.ToStream( dataOut );
        }

        protected virtual void WriteConsumerInfo( ConsumerInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "SUBSCRIBE", _encodeHeaders );

            if ( command.ResponseRequired )
                frame.SetProperty( PropertyKeys.Receipt, command.CommandId );

            frame.SetProperty( PropertyKeys.Destination, command.Destination?.ConvertToStompString() );
            frame.SetProperty( PropertyKeys.Id, command.ConsumerId.ToString() );
            frame.SetProperty( PropertyKeys.DurableSubscriberName, command.SubscriptionName );
            frame.SetProperty( PropertyKeys.Selector, command.Selector );
            frame.SetProperty( PropertyKeys.Ack, StompHelper.ToStomp( command.AckMode ) );

            if ( command.NoLocal )
                frame.SetProperty( PropertyKeys.NoLocal, command.NoLocal.ToString() );

            // ActiveMQ extensions to STOMP
            frame.SetProperty( PropertyKeys.Transformation, command.Transformation ?? "jms-xml" );

            frame.SetProperty( PropertyKeys.ActivemqDispatchAsync, command.DispatchAsync );

            if ( command.Exclusive )
                frame.SetProperty( PropertyKeys.ActivemqExclusive, command.Exclusive );

            if ( command.SubscriptionName != null )
            {
                frame.SetProperty( PropertyKeys.ActivemqSubscriptionName, command.SubscriptionName );
                // For an older 4.0 broker we need to set this header so they get the
                // subscription as well..
                frame.SetProperty( PropertyKeys.ActivemqSubcriptionName, command.SubscriptionName );
            }

            frame.SetProperty( PropertyKeys.ActivemqMaximumPendingMessageLimit, command.MaximumPendingMessageLimit );
            frame.SetProperty( PropertyKeys.ActivemqPrefetchSize, command.PrefetchSize );
            frame.SetProperty( PropertyKeys.ActivemqPriority, command.Priority );

            if ( command.Retroactive )
                frame.SetProperty( PropertyKeys.ActivemqRetroactive, command.Retroactive );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteKeepAliveInfo( KeepAliveInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( StompFrame.Keepalive, _encodeHeaders );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteMessage( BytesMessage command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "SEND", _encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( PropertyKeys.Receipt, command.CommandId );

            frame.SetProperty( PropertyKeys.Destination, command.Destination.ConvertToStompString() );

            if ( command.ReplyTo != null )
                frame.SetProperty( PropertyKeys.ReplyTo, command.ReplyTo.ConvertToStompString() );
            if ( command.CorrelationId != null )
                frame.SetProperty( PropertyKeys.CorrelationId, command.CorrelationId );
            if ( command.Expiration != 0 )
                frame.SetProperty( PropertyKeys.Expires, command.Expiration );
            if ( command.Timestamp != 0 )
                frame.SetProperty( PropertyKeys.TimeStamp, command.Timestamp );
            if ( command.Priority != 4 )
                frame.SetProperty( PropertyKeys.Priority, command.Priority );
            if ( command.Type != null )
                frame.SetProperty( PropertyKeys.Type, command.Type );
            if ( command.TransactionId != null )
                frame.SetProperty( PropertyKeys.Transaction, command.TransactionId.ToString() );

            frame.SetProperty( PropertyKeys.Persistent,
                               command.Persistent.ToString()
                                      .ToLower() );
            frame.SetProperty( PropertyKeys.NmsxDeliveryMode,
                               command.Persistent.ToString()
                                      .ToLower() );

            if ( command.StompGroupId != null )
            {
                frame.SetProperty( PropertyKeys.JmsxGroupId, command.StompGroupId );
                frame.SetProperty( PropertyKeys.NmsxGroupId, command.StompGroupId );
                frame.SetProperty( PropertyKeys.JmsxGroupSeq, command.StompGroupSeq );
                frame.SetProperty( PropertyKeys.NmsxGroupSeq, command.StompGroupSeq );
            }

            // Store the Marshaled Content.
            frame.Content = command.Content;

            if ( command.Content != null && command.Content.Length > 0 )
                frame.SetProperty( PropertyKeys.ContentLength, command.Content.Length );

            frame.SetProperty( PropertyKeys.Transformation, "jms-byte" );

            // Marshal all properties to the Frame.
            var map = command.Headers;
            foreach ( var key in map.Keys )
                frame.SetProperty( key, map[key] );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteMessageAck( MessageAck command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "ACK", _encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( PropertyKeys.Receipt, "ignore:" + command.CommandId );

            frame.SetProperty( PropertyKeys.MessageId, command.LastMessageId.ToString() );
            frame.SetProperty( PropertyKeys.Subscription, command.ConsumerId.ToString() );

            if ( command.TransactionId != null )
                frame.SetProperty( PropertyKeys.Transaction, command.TransactionId.ToString() );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteRemoveInfo( RemoveInfo command, BinaryWriter dataOut )
        {
            var frame = new StompFrame( "UNSUBSCRIBE", _encodeHeaders );
            Object id = command.ObjectId;

            if ( !( id is ConsumerId ) )
                return;
            var consumerId = id as ConsumerId;
            if ( command.ResponseRequired )
                frame.SetProperty( PropertyKeys.Receipt, command.CommandId );
            frame.SetProperty( PropertyKeys.Id, consumerId.ToString() );

            frame.ToStream( dataOut );
        }

        protected virtual void WriteShutdownInfo( ShutdownInfo command, BinaryWriter dataOut )
        {
            Debug.Assert( !command.ResponseRequired );

            var frame = new StompFrame( "DISCONNECT", _encodeHeaders );

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

            var frame = new StompFrame( type, _encodeHeaders );
            if ( command.ResponseRequired )
                frame.SetProperty( PropertyKeys.Receipt, command.CommandId );

            frame.SetProperty( PropertyKeys.Transaction, command.TransactionId.ToString() );

            frame.ToStream( dataOut );
        }
    }
}