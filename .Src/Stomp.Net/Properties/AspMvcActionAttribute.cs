#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter
    ///     is an MVC action. If applied to a method, the MVC action name is calculated
    ///     implicitly from the context. Use this attribute for custom wrappers similar to
    ///     <c>System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, String)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Method )]
    internal sealed class AspMvcActionAttribute : Attribute
    {
        #region Properties

        public String AnonymousProperty { get; private set; }

        #endregion

        #region Ctor

        public AspMvcActionAttribute()
        {
        }

        public AspMvcActionAttribute( String anonymousProperty )
        {
            AnonymousProperty = anonymousProperty;
        }

        #endregion
    }
}