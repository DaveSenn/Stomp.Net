#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates the condition parameter of the assertion method. The method itself should be
    ///     marked by <see cref="AssertionMethodAttribute" /> attribute. The mandatory argument of
    ///     the attribute is the assertion type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    internal sealed class AssertionConditionAttribute : Attribute
    {
        #region Properties

        public AssertionConditionType ConditionType { get; private set; }

        #endregion

        #region Ctor

        public AssertionConditionAttribute( AssertionConditionType conditionType )
        {
            ConditionType = conditionType;
        }

        #endregion
    }
}