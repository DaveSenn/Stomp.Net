#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     This attribute is intended to mark publicly available API
    ///     which should not be removed and so is treated as used.
    /// </summary>
    [MeansImplicitUse( ImplicitUseTargetFlags.WithMembers )]
    internal sealed class PublicAPIAttribute : Attribute
    {
        #region Properties

        public String Comment { get; private set; }

        #endregion

        #region Ctor

        public PublicAPIAttribute()
        {
        }

        public PublicAPIAttribute( [NotNull] String comment )
        {
            Comment = comment;
        }

        #endregion
    }
}