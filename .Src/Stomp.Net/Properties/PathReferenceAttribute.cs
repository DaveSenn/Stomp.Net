#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that a parameter is a path to a file or a folder within a web project.
    ///     Path can be relative or absolute, starting from web root (~).
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class PathReferenceAttribute : Attribute
    {
        #region Properties

        public String BasePath { get; private set; }

        #endregion

        #region Ctor

        public PathReferenceAttribute()
        {
        }

        public PathReferenceAttribute( [PathReference] String basePath )
        {
            BasePath = basePath;
        }

        #endregion
    }
}