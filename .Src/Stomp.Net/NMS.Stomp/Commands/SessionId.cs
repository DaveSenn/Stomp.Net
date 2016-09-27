#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class SessionId : BaseDataStructure
    {
        #region Fields

        private ConnectionId parentId;

        #endregion

        #region Properties

        public ConnectionId ParentId
        {
            get
            {
                if ( parentId == null )
                    parentId = new ConnectionId( this );
                return parentId;
            }
        }

        public String ConnectionId { get; set; }

        public Int64 Value { get; set; }

        #endregion

        #region Ctor

        public SessionId()
        {
        }

        public SessionId( ConnectionId connectionId, Int64 sessionId )
        {
            ConnectionId = connectionId.Value;
            Value = sessionId;
        }

        public SessionId( ProducerId producerId )
        {
            ConnectionId = producerId.ConnectionId;
            Value = producerId.SessionId;
        }

        public SessionId( ConsumerId consumerId )
        {
            ConnectionId = consumerId.ConnectionId;
            Value = consumerId.SessionId;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is SessionId )
                return Equals( (SessionId) that );
            return false;
        }

        public virtual Boolean Equals( SessionId that )
        {
            if ( !Equals( ConnectionId, that.ConnectionId ) )
                return false;
            if ( !Equals( Value, that.Value ) )
                return false;

            return true;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.SessionIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ConnectionId );
            answer = answer * 37 + HashCode( Value );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => ConnectionId + ":" + Value;
    }
}