#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class ConsumerId : BaseDataStructure
    {
        #region Fields

        private String _key;

        private SessionId _parentId;

        #endregion

        #region Properties

        public SessionId ParentId => _parentId ?? ( _parentId = new SessionId( this ) );

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
            _key = consumerKey;

            // We give the Connection ID the key for now so there's at least some
            // data stored into the Id.
            ConnectionId = consumerKey;

            var idx = consumerKey.LastIndexOf( ':' );
            if ( idx < 0 )
                return;
            try
            {
                Value = Int32.Parse( consumerKey.Substring( idx + 1 ) );
                consumerKey = consumerKey.Substring( 0, idx );
                idx = consumerKey.LastIndexOf( ':' );
                if ( idx >= 0 )
                {
                    SessionId = Int32.Parse( consumerKey.Substring( idx + 1 ) );
                    consumerKey = consumerKey.Substring( 0, idx );
                }
                ConnectionId = consumerKey;
            }
            catch ( Exception ex )
            {
                Tracer.Warn( $"Failed to get session id or consumer key '{ex}'." );
            }
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ConsumerId )
                return Equals( (ConsumerId) that );
            return false;
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
        public override String ToString() => _key ?? ( _key = ConnectionId + ":" + SessionId + ":" + Value );

        protected virtual Boolean Equals( ConsumerId that )
        {
            if ( _key != null && that._key != null )
                return _key.Equals( that._key );

            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;
            return Equals( SessionId, that.SessionId ) && Equals( Value, that.Value );
        }
    }
}