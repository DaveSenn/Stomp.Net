#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands;

public class RemoveInfo : BaseCommand
{
    public override Byte GetDataStructureType()
        => DataStructureTypes.RemoveInfoType;

    /// <summery>
    ///     Returns a string containing the information for this DataStructure
    ///     such as its type and value of its elements.
    /// </summery>
    public override String ToString()
        => GetType()
               .Name + "[" +
           "ObjectId=" + ObjectId +
           "]";

    #region Properties

    public IDataStructure ObjectId { get; set; }

    /// <summery>
    ///     Return an answer of true to the isRemoveInfo() query.
    /// </summery>
    public override Boolean IsRemoveInfo => true;

    #endregion
}