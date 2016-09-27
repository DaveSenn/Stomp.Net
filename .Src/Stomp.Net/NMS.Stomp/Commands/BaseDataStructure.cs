#region Usings

using System;
using Apache.NMS.Stomp.Protocol;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Base class for all DataStructure implementations
    /// </summary>
    public abstract class BaseDataStructure : DataStructure
    {
        public virtual Byte GetDataStructureType()
            => 0;

        public virtual Boolean IsMarshallAware()
            => false;

        public virtual Object Clone()
            => MemberwiseClone();

        public virtual void AfterMarshall( StompWireFormat wireFormat )
        {
        }

        public virtual void AfterUnmarshall( StompWireFormat wireFormat )
        {
        }

        public virtual void BeforeMarshall( StompWireFormat wireFormat )
        {
        }

        public virtual void BeforeUnmarshall( StompWireFormat wireFormat )
        {
        }

        public virtual Byte[] GetMarshalledForm( StompWireFormat wireFormat )
            => null;

        public virtual void SetMarshalledForm( StompWireFormat wireFormat, Byte[] data )
        {
        }

        // Helper methods
        protected static Int32 HashCode( Object value )
        {
            if ( value != null )
                return value.GetHashCode();

            return -1;
        }
    }
}