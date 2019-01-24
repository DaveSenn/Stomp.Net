#region Usings

using System;
using System.Collections.Concurrent;
using System.Linq;
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
                _requestMap.AddOrUpdate( commandId, future, ( k, v ) => future );

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

        #region Overrides of Disposable

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected override void Disposed()
            => Dispose( null );

        #endregion

        protected override void OnCommand( ITransport sender, ICommand command )
        {
            if ( command is Response response )
            {
                var correlationId = response.CorrelationId;

                if ( _requestMap.TryGetValue( correlationId, out var future ) )
                {
                    if ( !_requestMap.TryRemove( correlationId, out _ ) && Tracer.IsWarnEnabled )
                        Tracer.Warn( $"Failed to remove future response with id: '{correlationId}'." );

                    future.Response = response;

                    if ( !( response is ExceptionResponse ) )
                        return;

                    var er = response as ExceptionResponse;
                    if ( Tracer.IsErrorEnabled )
                        Tracer.Error( $"Response is exception response, exception: {er.Exception} ({er.Exception.Message})" );

                    var brokerError = er.Exception;
                    var exception = new BrokerException( brokerError );
                    Exception( this, exception );
                }
                else
                {
                    if ( Tracer.IsWarnEnabled )
                        Tracer.Warn( $"Unknown response ID: {response.CommandId} for response: {response}" );
                    if ( !( response is ExceptionResponse exResponse ) )
                        return;

                    if ( Tracer.IsErrorEnabled )
                        Tracer.Error( $"Response is exception response, exception: {exResponse.Exception} ({exResponse.Exception.Message})" );
                    var exception = new BrokerException( exResponse.Exception );
                    Exception( this, exception );
                }
            }
            else
                Command( sender, command );
        }

        protected override void OnException( ITransport sender, Exception command )
        {
            Dispose( command );
            base.OnException( sender, command );
        }

        private void Dispose( Exception error )
        {
            var requests = _requestMap.Values.ToList();

            if ( _error == null )
            {
                _error = error;
                _requestMap.Clear();
            }

            foreach ( var future in requests )
            {
                var brError = new BrokerError { Message = error?.Message };
                var response = new ExceptionResponse { Exception = brError };
                future.Response = response;
            }
        }

        private Int32 GetNextCommandId()
            => Interlocked.Increment( ref _nextCommandId );

        #region Fields

        private readonly ConcurrentDictionary<Int32, FutureResponse> _requestMap = new ConcurrentDictionary<Int32, FutureResponse>();

        private Exception _error;
        private Int32 _nextCommandId;

        #endregion
    }
}