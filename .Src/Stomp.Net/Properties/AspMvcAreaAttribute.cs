#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     ASP.NET MVC attribute. Indicates that a parameter is an MVC area.
    ///     Use this attribute for custom wrappers similar to
    ///     <c>System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, String)</c>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class AspMvcAreaAttribute : Attribute
    {
        #region Properties

        public String AnonymousProperty { get; private set; }

        #endregion

        #region Ctor

        public AspMvcAreaAttribute()
        {
        }

        public AspMvcAreaAttribute( String anonymousProperty )
        {
            AnonymousProperty = anonymousProperty;
        }

        #endregion
    }
}