

#region Usings

using System;
using Apache.NMS.Stomp.Protocol;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Base class for all DataStructure implementations
    /// </summary>
    public abstract class BaseDataStructure : DataStructure, ICloneable
    {
        public virtual Byte GetDataStructureType()
        {
            return 0;
        }

        public virtual Boolean IsMarshallAware()
        {
            return false;
        }

        public virtual Object Clone()
        {
            // Since we are the lowest level base class, do a
            // shallow copy which will include the derived classes.
            // From here we would do deep cloning of other objects
            // if we had any.
            return MemberwiseClone();
        }

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
        {
            return null;
        }

        // Helper methods
        public Int32 HashCode( Object value )
        {
            if ( value != null )
                return value.GetHashCode();

            return -1;
        }

        public virtual void SetMarshalledForm( StompWireFormat wireFormat, Byte[] data )
        {
        }
    }
}