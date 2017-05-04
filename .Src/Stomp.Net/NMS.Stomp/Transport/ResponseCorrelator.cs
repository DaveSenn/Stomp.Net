#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        
        private readonly ConcurrentDictionary<Int32, FutureResponse> _requestMap = new ConcurrentDictionary<Int32, FutureResponse>();
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

            var priorError = _error;
                if ( priorError == null )
                    _requestMap.AddOrUpdate( commandId, future, ( key, value ) => future );
            

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
                _requestMap.TryGetValue( correlationId, out FutureResponse future );

                if ( future != null )
                {
                    _requestMap.TryRemove( correlationId, out future );
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
                Command( sender, command );
        }

        protected override void OnException( ITransport sender, Exception command )
        {
            Dispose( command );
            base.OnException( sender, command );
        }


        private Int32 GetNextCommandId()
            => Interlocked.Increment( ref _nextCommandId );

        #region Overrides of Disposable

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed() 
            => Dispose( null );

        #endregion

        private void Dispose( Exception error )
        {
            List<FutureResponse> requests = null;

            if ( _error == null )
            {
                _error = error;
                requests = new List<FutureResponse>( _requestMap.Values );
                _requestMap.Clear();
            }

            if ( requests == null )
                return;

            foreach ( var future in requests )
            {
                var brError = new BrokerError { Message = error?.Message };
                var response = new ExceptionResponse { Exception = brError };
                future.Response = response;
            }
        }
    }
}
