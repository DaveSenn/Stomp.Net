#region Usings

using System;
using Apache.NMS.Stomp.State;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class KeepAliveInfo : BaseCommand
    {
        #region Properties

        /// <summery>
        ///     Return an answer of true to the isKeepAliveInfo() query.
        /// </summery>
        public override Boolean IsKeepAliveInfo
        {
            get { return true; }
        }

        #endregion

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.KeepAliveInfoType;

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString() => GetType()
                                                 .Name + "[ " +
                                             "commandId = " + CommandId + ", " +
                                             "responseRequired = " + ResponseRequired + ", " + " ]";

        /// <summery>
        ///     Allows a Visitor to Visit this command and return a response to the
        ///     command based on the command type being visited.  The command will call
        ///     the proper processXXX method in the visitor.
        /// </summery>
        public override Response Visit( ICommandVisitor visitor ) => visitor.ProcessKeepAliveInfo( this );
    }
}