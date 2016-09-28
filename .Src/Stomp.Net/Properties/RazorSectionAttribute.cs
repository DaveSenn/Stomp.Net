﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Razor attribute. Indicates that a parameter or a method is a Razor section.
    ///     Use this attribute for custom wrappers similar to
    ///     <c>System.Web.WebPages.WebPageBase.RenderSection(String)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Method )]
    internal sealed class RazorSectionAttribute : Attribute
    {
    }
}