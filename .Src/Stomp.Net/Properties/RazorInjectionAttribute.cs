#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    internal sealed class RazorInjectionAttribute : Attribute
    {
        #region Properties

        public String Type { get; private set; }
        public String FieldName { get; private set; }

        #endregion

        #region Ctor

        public RazorInjectionAttribute( String type, String fieldName )
        {
            Type = type;
            FieldName = fieldName;
        }

        #endregion
    }
}