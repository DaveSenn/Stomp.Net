#region Usings

using System;
using System.Collections;
using System.Threading;
using Apache.NMS.Stomp.Commands;
using Stomp.Net;

#endregion

namespace Apache.NMS.Stomp.Transport
{
    /// <summary>
    ///     A Transport that correlates asynchronous send/receive messages into single request/response.
    /// </summary>
    public class ResponseCorrelator : TransportFilter
    {
        #region Fields

        private readonly IDictionary requestMap = Hashtable.Synchronized( new Hashtable() );
        private Exception error;
        private Int32 nextCommandId;

        #endregion

        #region Ctor

        public ResponseCorrelator( ITransport next )
            : base( next )
        {
        }

        #endregion

        public override FutureResponse AsyncRequest( ICommand command )
        {
            var commandId = GetNextCommandId();

            command.CommandId = commandId;
            command.ResponseRequired = true;
            var future = new FutureResponse();
            Exception priorError;
            lock ( requestMap.SyncRoot )
            {
                priorError = error;
                if ( priorError == null )
                    requestMap[commandId] = future;
            }

            if ( priorError != null )
            {
                var brError = new BrokerError { Message = priorError.Message };
                var response = new ExceptionResponse { Exception = brError };
                future.Response = response;
                throw priorError;
            }

            next.Oneway( command );

            return future;
        }

        public override void Oneway( ICommand command )
        {
            command.CommandId = GetNextCommandId();
            command.ResponseRequired = false;

            next.Oneway( command );
        }

        public override Response Request( ICommand command, TimeSpan timeout )
        {
            var future = AsyncRequest( command );
            future.ResponseTimeout = timeout;
            var response = future.Response;

            if ( response != null && response is ExceptionResponse )
            {
                var er = response as ExceptionResponse;
                var brokerError = er.Exception;

                if ( brokerError == null )
                    throw new BrokerException();

                throw new BrokerException( brokerError );
            }

            return response;
        }

        public override void Stop()
        {
            Dispose( new IOException( "Stopped" ) );
            base.Stop();
        }

        protected override void OnCommand( ITransport sender, ICommand command )
        {
            if ( command is Response )
            {
                var response = (Response) command;
                var correlationId = response.CorrelationId;
                var future = (FutureResponse) requestMap[correlationId];

                if ( future != null )
                {
                    requestMap.Remove( correlationId );
                    future.Response = response;

                    if ( response is ExceptionResponse )
                    {
                        var er = response as ExceptionResponse;
                        var brokerError = er.Exception;
                        var exception = new BrokerException( brokerError );
                        Exception( this, exception );
                    }
                }
                else
                {
                    if ( Tracer.IsDebugEnabled )
                        Tracer.Debug( "Unknown response ID: " + response.CommandId + " for response: " + response );
                }
            }
            else
            {
                Command( sender, command );
            }
        }

        protected override void OnException( ITransport sender, Exception command )
        {
            Dispose( command );
            base.OnException( sender, command );
        }

        internal Int32 GetNextCommandId() => Interlocked.Increment( ref nextCommandId );

        private void Dispose( Exception error )
        {
            ArrayList requests = null;

            lock ( requestMap.SyncRoot )
                if ( this.error == null )
                {
                    this.error = error;
                    requests = new ArrayList( requestMap.Values );
                    requestMap.Clear();
                }

            if ( requests != null )
                foreach ( FutureResponse future in requests )
                {
                    var brError = new BrokerError();
                    brError.Message = error.Message;
                    var response = new ExceptionResponse();
                    response.Exception = brError;
                    future.Response = response;
                }
        }
    }
}