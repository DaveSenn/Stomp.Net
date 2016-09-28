#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter
    ///     is an MVC view component name.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class AspMvcViewComponentAttribute : Attribute
    {
    }
}