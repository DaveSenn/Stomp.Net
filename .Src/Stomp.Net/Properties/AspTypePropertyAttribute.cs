#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Property )]
    internal sealed class AspTypePropertyAttribute : Attribute
    {
        #region Properties

        public Boolean CreateConstructorReferences { get; private set; }

        #endregion

        #region Ctor

        public AspTypePropertyAttribute( Boolean createConstructorReferences )
        {
            CreateConstructorReferences = createConstructorReferences;
        }

        #endregion
    }
}