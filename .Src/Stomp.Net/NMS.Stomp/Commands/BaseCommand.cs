#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public abstract class BaseCommand : BaseDataStructure, ICommand, ICloneable
    {
        #region Properties

        public virtual Boolean IsBrokerInfo
        {
            get { return false; }
        }

        public virtual Boolean IsConnectionError
        {
            get { return false; }
        }

        public virtual Boolean IsConsumerInfo
        {
            get { return false; }
        }

        public virtual Boolean IsControlCommand
        {
            get { return false; }
        }

        public virtual Boolean IsProducerAck
        {
            get { return false; }
        }

        public virtual Boolean IsProducerInfo
        {
            get { return false; }
        }

        public virtual Boolean IsSessionInfo
        {
            get { return false; }
        }

        public virtual Boolean IsTransactionInfo
        {
            get { return false; }
        }

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

        public virtual Boolean IsConnectionInfo
        {
            get { return false; }
        }

        public virtual Boolean IsDestinationInfo
        {
            get { return false; }
        }

        public virtual Boolean IsErrorCommand
        {
            get { return false; }
        }

        public virtual Boolean IsKeepAliveInfo
        {
            get { return false; }
        }

        public virtual Boolean IsMessage
        {
            get { return false; }
        }

        public virtual Boolean IsMessageAck
        {
            get { return false; }
        }

        public virtual Boolean IsMessageDispatch
        {
            get { return false; }
        }

        public virtual Boolean IsRemoveInfo
        {
            get { return false; }
        }

        public virtual Boolean IsRemoveSubscriptionInfo
        {
            get { return false; }
        }

        public virtual Boolean IsResponse
        {
            get { return false; }
        }

        public virtual Boolean IsShutdownInfo
        {
            get { return false; }
        }

        public virtual Boolean IsWireFormatInfo
        {
            get { return false; }
        }

        public virtual Boolean ResponseRequired { get; set; } = false;

        public virtual Response visit( ICommandVisitor visitor )
        {
            throw new ApplicationException( "BaseCommand.Visit() not implemented" );
        }

        public override Boolean Equals( Object that )
        {
            if ( that is BaseCommand )
            {
                var thatCommand = (BaseCommand) that;
                return GetDataStructureType() == thatCommand.GetDataStructureType()
                       && CommandId == thatCommand.CommandId;
            }
            return false;
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