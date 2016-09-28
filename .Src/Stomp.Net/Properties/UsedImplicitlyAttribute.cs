#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
    ///     so this symbol will not be marked as unused (as well as by other usage inspections).
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    internal sealed class UsedImplicitlyAttribute : Attribute
    {
        #region Properties

        public ImplicitUseKindFlags UseKindFlags { get; private set; }
        public ImplicitUseTargetFlags TargetFlags { get; private set; }

        #endregion

        #region Ctor

        public UsedImplicitlyAttribute()
            : this( ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default )
        {
        }

        public UsedImplicitlyAttribute( ImplicitUseKindFlags useKindFlags )
            : this( useKindFlags, ImplicitUseTargetFlags.Default )
        {
        }

        public UsedImplicitlyAttribute( ImplicitUseTargetFlags targetFlags )
            : this( ImplicitUseKindFlags.Default, targetFlags )
        {
        }

        public UsedImplicitlyAttribute( ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags )
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        #endregion
    }
}