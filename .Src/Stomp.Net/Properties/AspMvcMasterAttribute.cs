﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. Indicates that a parameter is an MVC Master. Use this attribute
    ///     for custom wrappers similar to <c>System.Web.Mvc.Controller.View(String, String)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class AspMvcMasterAttribute : Attribute
    {
    }
}