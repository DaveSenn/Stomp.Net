#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that parameter is regular expression pattern.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class RegexPatternAttribute : Attribute
    {
    }
}