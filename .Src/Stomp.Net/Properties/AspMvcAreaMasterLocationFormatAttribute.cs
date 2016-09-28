#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    internal sealed class AspMvcAreaMasterLocationFormatAttribute : Attribute
    {
        #region Properties

        public String Format { get; private set; }

        #endregion

        #region Ctor

        public AspMvcAreaMasterLocationFormatAttribute( String format )
        {
            Format = format;
        }

        #endregion
    }
}