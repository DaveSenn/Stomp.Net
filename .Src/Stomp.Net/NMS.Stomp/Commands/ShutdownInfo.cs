#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class ShutdownInfo : BaseCommand
    {
        #region Properties

        /// <summery>
        ///     Return an answer of true to the isShutdownInfo() query.
        /// </summery>
        public override Boolean IsShutdownInfo
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.ShutdownInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[" + "]";

        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessShutdownInfo( this );
    }
}