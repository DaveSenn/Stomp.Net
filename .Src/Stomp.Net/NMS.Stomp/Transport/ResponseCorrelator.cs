#region Usings

using System;
using System.Collections;
using System.Threading;
using Stomp.Net.Stomp.Commands;

#endregion

namespace Stomp.Net.Stomp.Transport
{
    /// <summary>
    ///     A Transport that correlates asynchronous send/receive messages into single request/response.
    /// </summary>
    public class ResponseCorrelator : TransportFilter
    {
        #region Fields

        private readonly IDictionary _requestMap = Hashtable.Synchronized( new Hashtable() );
        private Exception _error;
        private Int32 _nextCommandId;

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
            lock ( _requestMap.SyncRoot )
            {
                priorError = _error;
                if ( priorError == null )
                    _requestMap[commandId] = future;
            }

            if ( priorError != null )
            {
                var brError = new BrokerError { Message = priorError.Message };
                var response = new ExceptionResponse { Exception = brError };
                future.Response = response;
                throw priorError;
            }

            Next.Oneway( command );

            return future;
        }

        public override void Oneway( ICommand command )
        {
            command.CommandId = GetNextCommandId();
            command.ResponseRequired = false;

            Next.Oneway( command );
        }

        public override Response Request( ICommand command, TimeSpan timeout )
        {
            var future = AsyncRequest( command );
            future.ResponseTimeout = timeout;
            var response = future.Response;

            if ( !( response is ExceptionResponse ) )
                return response;
            var er = response as ExceptionResponse;
            var brokerError = er.Exception;

            if ( brokerError == null )
                throw new BrokerException();

            throw new BrokerException( brokerError );
        }

        public override void Stop()
        {
            Dispose( new IoException( "Stopped" ) );
            base.Stop();
        }

        protected override void OnCommand( ITransport sender, ICommand command )
        {
            if ( command is Response )
            {
                var response = (Response) command;
                var correlationId = response.CorrelationId;
                var future = (FutureResponse) _requestMap[correlationId];

                if ( future != null )
                {
                    _requestMap.Remove( correlationId );
                    future.Response = response;

                    if ( !( response is ExceptionResponse ) )
                        return;
                    var er = response as ExceptionResponse;
                    var brokerError = er.Exception;
                    var exception = new BrokerException( brokerError );
                    Exception( this, exception );
                }
                else
                    Tracer.Warn( "Unknown response ID: " + response.CommandId + " for response: " + response );
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

        private void Dispose( Exception error )
        {
            ArrayList requests = null;

            lock ( _requestMap.SyncRoot )
                if ( _error == null )
                {
                    _error = error;
                    requests = new ArrayList( _requestMap.Values );
                    _requestMap.Clear();
                }

            if ( requests == null )
                return;
            foreach ( FutureResponse future in requests )
            {
                var brError = new BrokerError { Message = error.Message };
                var response = new ExceptionResponse { Exception = brError };
                future.Response = response;
            }
        }

        private Int32 GetNextCommandId()
            => Interlocked.Increment( ref _nextCommandId );
    }
}