#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ConnectionId : BaseDataStructure
    {
        #region Properties

        public String Value { get; set; }

        #endregion

        #region Ctor

        public ConnectionId()
        {
        }

        public ConnectionId( SessionId sessionId )
        {
            Value = sessionId.ConnectionId;
        }

        public ConnectionId( ProducerId producerId )
        {
            Value = producerId.ConnectionId;
        }

        public ConnectionId( ConsumerId consumerId )
        {
            Value = consumerId.ConnectionId;
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ConnectionId )
                return Equals( (ConnectionId) that );
            return false;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ConnectionIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( Value );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => Value;

        protected virtual Boolean Equals( ConnectionId that ) => Equals( Value, that.Value );
    }
}