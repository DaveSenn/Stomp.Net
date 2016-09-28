#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that the return value of method invocation must be used.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    internal sealed class MustUseReturnValueAttribute : Attribute
    {
        #region Properties

        public String Justification { get; private set; }

        #endregion

        #region Ctor

        public MustUseReturnValueAttribute()
        {
        }

        public MustUseReturnValueAttribute( [NotNull] String justification )
        {
            Justification = justification;
        }

        #endregion
    }
}