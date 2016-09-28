#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that the marked method builds string by format pattern and (optional) arguments.
    ///     Parameter, which contains format string, should be given in constructor. The format string
    ///     should be in <see cref="string.Format(IFormatProvider,string,object[])" />-like form.
    /// </summary>
    /// <example>
    ///     <code>
    /// [StringFormatMethod("message")]
    /// void ShowError(string message, params object[] args) { /* do something */ }
    /// 
    /// void Foo() {
    ///   ShowError("Failed: {0}"); // Warning: Non-existing argument in format string
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(
         AttributeTargets.Constructor | AttributeTargets.Method |
         AttributeTargets.Property | AttributeTargets.Delegate )]
    internal sealed class StringFormatMethodAttribute : Attribute
    {
        #region Properties

        public String FormatParameterName { get; private set; }

        #endregion

        #region Ctor

        /// <param name="formatParameterName">
        ///     Specifies which parameter of an annotated method should be treated as format-string
        /// </param>
        public StringFormatMethodAttribute( String formatParameterName )
        {
            FormatParameterName = formatParameterName;
        }

        #endregion
    }
}