#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Method )]
    internal sealed class AspDataFieldsAttribute : Attribute
    {
    }
}