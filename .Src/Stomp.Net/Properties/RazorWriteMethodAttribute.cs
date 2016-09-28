#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Method )]
    internal sealed class RazorWriteMethodAttribute : Attribute
    {
    }
}