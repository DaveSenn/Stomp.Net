#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class ConnectionId : BaseDataStructure
    {
        #region Properties

        public String Value { get; set; }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is ConnectionId id )
                return Equals( id );

            return false;
        }

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType()
            => DataStructureTypes.ConnectionIdType;

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
        public override String ToString()
            => Value;

        protected virtual Boolean Equals( ConnectionId that )
            => Equals( Value, that.Value );
    }
}