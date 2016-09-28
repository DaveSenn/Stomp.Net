#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter is
    ///     an MVC controller. If applied to a method, the MVC controller name is calculated
    ///     implicitly from the context. Use this attribute for custom wrappers similar to
    ///     <c>System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, String, String)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Method )]
    internal sealed class AspMvcControllerAttribute : Attribute
    {
        #region Properties

        public String AnonymousProperty { get; private set; }

        #endregion

        #region Ctor

        public AspMvcControllerAttribute()
        {
        }

        public AspMvcControllerAttribute( String anonymousProperty )
        {
            AnonymousProperty = anonymousProperty;
        }

        #endregion
    }
}