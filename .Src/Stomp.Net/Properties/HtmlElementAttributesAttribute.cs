#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field )]
    internal sealed class HtmlElementAttributesAttribute : Attribute
    {
        #region Properties

        public String Name { get; private set; }

        #endregion

        #region Ctor

        public HtmlElementAttributesAttribute()
        {
        }

        public HtmlElementAttributesAttribute( String name )
        {
            Name = name;
        }

        #endregion
    }
}