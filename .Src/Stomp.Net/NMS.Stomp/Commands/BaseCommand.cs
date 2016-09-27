#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public abstract class BaseCommand : BaseDataStructure, ICommand
    {
        #region Properties

        public virtual Boolean IsMessage => false;

        public virtual Boolean IsMessageAck => false;

        public virtual Boolean IsRemoveSubscriptionInfo => false;

        #endregion

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (BaseCommand) base.Clone();

            return o;
        }

        public Int32 CommandId { get; set; }

        public virtual Boolean IsConnectionInfo => false;

        public virtual Boolean IsErrorCommand => false;

        public virtual Boolean IsKeepAliveInfo => false;

        public virtual Boolean IsMessageDispatch => false;

        public virtual Boolean IsRemoveInfo => false;

        public virtual Boolean IsResponse => false;

        public virtual Boolean IsShutdownInfo => false;

        public virtual Boolean IsWireFormatInfo => false;

        public virtual Boolean ResponseRequired { get; set; } = false;

        public virtual Response Visit( ICommandVisitor visitor )
        {
            throw new ApplicationException( "BaseCommand.Visit() not implemented" );
        }

        public override Boolean Equals( Object that )
        {
            if ( !( that is BaseCommand ) )
                return false;

            var thatCommand = (BaseCommand) that;
            return GetDataStructureType() == thatCommand.GetDataStructureType()
                   && CommandId == thatCommand.CommandId;
        }

        public override Int32 GetHashCode() => CommandId * 37 + GetDataStructureType();

        public override String ToString()
        {
            var answer = DataStructureTypes.GetDataStructureTypeAsString( GetDataStructureType() );
            if ( answer.Length == 0 )
                answer = base.ToString();

            return answer + ": id = " + CommandId;
        }
    }
}