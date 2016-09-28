#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Should be used on attributes and causes ReSharper to not mark symbols marked with such attributes
    ///     as unused (as well as by other usage inspections)
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.GenericParameter )]
    internal sealed class MeansImplicitUseAttribute : Attribute
    {
        #region Properties

        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }

        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }

        #endregion

        #region Ctor

        public MeansImplicitUseAttribute()
            : this( ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default )
        {
        }

        public MeansImplicitUseAttribute( ImplicitUseKindFlags useKindFlags )
            : this( useKindFlags, ImplicitUseTargetFlags.Default )
        {
        }

        public MeansImplicitUseAttribute( ImplicitUseTargetFlags targetFlags )
            : this( ImplicitUseKindFlags.Default, targetFlags )
        {
        }

        public MeansImplicitUseAttribute( ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags )
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        #endregion
    }
}