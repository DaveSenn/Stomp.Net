#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    internal sealed class AspRequiredAttributeAttribute : Attribute
    {
        #region Properties

        public String Attribute { get; private set; }

        #endregion

        #region Ctor

        public AspRequiredAttributeAttribute( [NotNull] String attribute )
        {
            Attribute = attribute;
        }

        #endregion
    }
}