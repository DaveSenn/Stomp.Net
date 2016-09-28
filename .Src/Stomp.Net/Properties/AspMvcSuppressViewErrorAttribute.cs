#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. Allows disabling inspections for MVC views within a class or a method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    internal sealed class AspMvcSuppressViewErrorAttribute : Attribute
    {
    }
}