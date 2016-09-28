#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that marked element should be localized or not.
    /// </summary>
    /// <example>
    ///     <code>
    /// [LocalizationRequiredAttribute(true)]
    /// class Foo {
    ///   string str = "my string"; // Warning: Localizable string
    /// }
    /// </code>
    /// </example>
    [AttributeUsage( AttributeTargets.All )]
    internal sealed class LocalizationRequiredAttribute : Attribute
    {
        #region Properties

        public Boolean Required { get; private set; }

        #endregion

        #region Ctor

        public LocalizationRequiredAttribute()
            : this( true )
        {
        }

        public LocalizationRequiredAttribute( Boolean required )
        {
            Required = required;
        }

        #endregion
    }
}