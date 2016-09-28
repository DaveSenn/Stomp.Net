﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. When applied to a parameter of an attribute,
    ///     indicates that this parameter is an MVC action name.
    /// </summary>
    /// <example>
    ///     <code>
    /// [ActionName("Foo")]
    /// public ActionResult Login(string returnUrl) {
    ///   ViewBag.ReturnUrl = Url.Action("Foo"); // OK
    ///   return RedirectToAction("Bar"); // Error: Cannot resolve action
    /// }
    /// </code>
    /// </example>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Property )]
    internal sealed class AspMvcActionSelectorAttribute : Attribute
    {
    }
}