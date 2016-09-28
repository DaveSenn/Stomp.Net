#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class TransactionId : BaseDataStructure
    {
        #region Properties

        public Int64 Value { get; set; }

        public ConnectionId ConnectionId { get; set; }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is TransactionId )
                return Equals( (TransactionId) that );
            return false;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.TransactionIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( Value );
            answer = answer * 37 + HashCode( ConnectionId );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => ConnectionId + ":" + Value;

        protected virtual Boolean Equals( TransactionId that )
            => Equals( Value, that.Value ) && Equals( ConnectionId, that.ConnectionId );
    }
}