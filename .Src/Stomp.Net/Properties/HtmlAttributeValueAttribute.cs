#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
    internal sealed class HtmlAttributeValueAttribute : Attribute
    {
        #region Properties

        [NotNull]
        public String Name { get; private set; }

        #endregion

        #region Ctor

        public HtmlAttributeValueAttribute( [NotNull] String name )
        {
            Name = name;
        }

        #endregion
    }
}