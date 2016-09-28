#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    internal sealed class RazorImportNamespaceAttribute : Attribute
    {
        #region Properties

        public String Name { get; private set; }

        #endregion

        #region Ctor

        public RazorImportNamespaceAttribute( String name )
        {
            Name = name;
        }

        #endregion
    }
}