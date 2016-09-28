#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    internal sealed class AspChildControlTypeAttribute : Attribute
    {
        #region Properties

        public String TagName { get; private set; }
        public Type ControlType { get; private set; }

        #endregion

        #region Ctor

        public AspChildControlTypeAttribute( String tagName, Type controlType )
        {
            TagName = tagName;
            ControlType = controlType;
        }

        #endregion
    }
}