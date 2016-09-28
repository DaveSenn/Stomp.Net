#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Property )]
    internal sealed class RazorLayoutAttribute : Attribute
    {
    }
}