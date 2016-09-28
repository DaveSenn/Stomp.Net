#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    internal sealed class AspMvcMasterLocationFormatAttribute : Attribute
    {
        #region Properties

        public String Format { get; private set; }

        #endregion

        #region Ctor

        public AspMvcMasterLocationFormatAttribute( String format )
        {
            Format = format;
        }

        #endregion
    }
}