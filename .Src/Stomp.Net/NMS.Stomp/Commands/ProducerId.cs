#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ProducerId : BaseDataStructure
    {
        #region Fields

        private String _key;
        private SessionId _parentId;

        #endregion

        #region Properties

        public SessionId ParentId => _parentId ?? ( _parentId = new SessionId( this ) );

        public String ConnectionId { get; set; }

        public Int64 Value { get; set; }

        public Int64 SessionId { get; set; }

        #endregion

        #region Ctor

        public ProducerId()
        {
        }

        public ProducerId( SessionId sessionId, Int64 consumerId )
        {
            ConnectionId = sessionId.ConnectionId;
            SessionId = sessionId.Value;
            Value = consumerId;
        }

        public ProducerId( String producerKey )
        {
            // Store the original.
            _key = producerKey;

            // Try and get back the AMQ version of the data.
            var idx = producerKey.LastIndexOf( ':' );
            if ( idx >= 0 )
                try
                {
                    Value = Int32.Parse( producerKey.Substring( idx + 1 ) );
                    producerKey = producerKey.Substring( 0, idx );
                    idx = producerKey.LastIndexOf( ':' );
                    if ( idx >= 0 )
                    {
                        SessionId = Int32.Parse( producerKey.Substring( idx + 1 ) );
                        producerKey = producerKey.Substring( 0, idx );
                    }
                }
                catch ( Exception ex )
                {
                    Tracer.Warn( ex.Message );
                }
            ConnectionId = producerKey;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ProducerId )
                return Equals( (ProducerId) that );
            return false;
        }

        public virtual Boolean Equals( ProducerId that )
        {
            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;
            if ( !Equals( Value, that.Value ) )
                return false;
            if ( !Equals( SessionId, that.SessionId ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ProducerIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ConnectionId );
            answer = answer * 37 + HashCode( Value );
            answer = answer * 37 + HashCode( SessionId );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() 
            => _key ?? ( _key = ConnectionId + ":" + SessionId + ":" + Value );
    }
}