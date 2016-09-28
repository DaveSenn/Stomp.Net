#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class RazorWriteMethodParameterAttribute : Attribute
    {
    }
}