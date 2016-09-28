#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Base class for all DataStructure implementations
    /// </summary>
    public abstract class BaseDataStructure : IDataStructure
    {
        public virtual Object Clone()
            => MemberwiseClone();

        public virtual Byte GetDataStructureType()
            => 0;

        // Helper methods
        protected static Int32 HashCode( Object value )
        {
            if ( value != null )
                return value.GetHashCode();

            return -1;
        }
    }
}