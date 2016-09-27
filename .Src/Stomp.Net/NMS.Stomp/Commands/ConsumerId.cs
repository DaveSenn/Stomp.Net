#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ConsumerId : BaseDataStructure
    {
        #region Fields

        private String key;

        private SessionId parentId;

        #endregion

        #region Properties

        public SessionId ParentId
        {
            get
            {
                if ( parentId == null )
                    parentId = new SessionId( this );
                return parentId;
            }
        }

        public String ConnectionId { get; set; }

        public Int64 SessionId { get; set; }

        public Int64 Value { get; set; }

        #endregion

        #region Ctor

        public ConsumerId()
        {
        }

        public ConsumerId( String consumerKey )
        {
            key = consumerKey;

            // We give the Connection ID the key for now so there's at least some
            // data stored into the Id.
            ConnectionId = consumerKey;

            var idx = consumerKey.LastIndexOf( ':' );
            if ( idx >= 0 )
                try
                {
                    Value = Int32.Parse( consumerKey.Substring( idx + 1 ) );
                    consumerKey = consumerKey.Substring( 0, idx );
                    idx = consumerKey.LastIndexOf( ':' );
                    if ( idx >= 0 )
                        try
                        {
                            SessionId = Int32.Parse( consumerKey.Substring( idx + 1 ) );
                            consumerKey = consumerKey.Substring( 0, idx );
                        }
                        catch ( Exception ex )
                        {
                            Tracer.Warn( ex.Message );
                        }
                    ConnectionId = consumerKey;
                }
                catch ( Exception ex )
                {
                    Tracer.Warn( ex.Message );
                }
        }

        public ConsumerId( SessionId sessionId, Int64 consumerId )
        {
            ConnectionId = sessionId.ConnectionId;
            SessionId = sessionId.Value;
            Value = consumerId;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ConsumerId )
                return Equals( (ConsumerId) that );
            return false;
        }

        public virtual Boolean Equals( ConsumerId that )
        {
            if ( key != null && that.key != null )
                return key.Equals( that.key );

            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;
            if ( !Equals( SessionId, that.SessionId ) )
                return false;
            if ( !Equals( Value, that.Value ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ConsumerIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ConnectionId );
            answer = answer * 37 + HashCode( SessionId );
            answer = answer * 37 + HashCode( Value );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
        {
            if ( key == null )
                key = ConnectionId + ":" + SessionId + ":" + Value;

            return key;
        }
    }
}