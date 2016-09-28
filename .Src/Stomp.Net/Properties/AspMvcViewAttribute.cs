﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter
    ///     is an MVC view component. If applied to a method, the MVC view name is calculated implicitly
    ///     from the context. Use this attribute for custom wrappers similar to
    ///     <c>System.Web.Mvc.Controller.View(Object)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Method )]
    internal sealed class AspMvcViewAttribute : Attribute
    {
    }
}