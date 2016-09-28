#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     For a parameter that is expected to be one of the limited set of values.
    ///     Specify fields of which type should be used as values for this parameter.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field )]
    internal sealed class ValueProviderAttribute : Attribute
    {
        #region Properties

        [NotNull]
        public String Name { get; private set; }

        #endregion

        #region Ctor

        public ValueProviderAttribute( String name )
        {
            Name = name;
        }

        #endregion
    }
}