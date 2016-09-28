#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that the marked method unconditionally terminates control flow execution.
    ///     For example, it could unconditionally throw exception.
    /// </summary>
    [Obsolete( "Use [ContractAnnotation('=> halt')] instead" )]
    [AttributeUsage( AttributeTargets.Method )]
    internal sealed class TerminatesProgramAttribute : Attribute
    {
    }
}