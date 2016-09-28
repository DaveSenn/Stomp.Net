#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that IEnumerable, passed as parameter, is not enumerated.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class NoEnumerationAttribute : Attribute
    {
    }
}