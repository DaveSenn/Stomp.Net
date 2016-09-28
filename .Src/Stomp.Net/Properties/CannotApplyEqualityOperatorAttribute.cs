﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates that the value of the marked type (or its derivatives)
    ///     cannot be compared using '==' or '!=' operators and <c>Equals()</c>
    ///     should be used instead. However, using '==' or '!=' for comparison
    ///     with <c>null</c> is always permitted.
    /// </summary>
    /// <example>
    ///     <code>
    /// [CannotApplyEqualityOperator]
    /// class NoEquality { }
    /// 
    /// class UsesNoEquality {
    ///   void Test() {
    ///     var ca1 = new NoEquality();
    ///     var ca2 = new NoEquality();
    ///     if (ca1 != null) { // OK
    ///       bool condition = ca1 == ca2; // Warning
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage( AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct )]
    internal sealed class CannotApplyEqualityOperatorAttribute : Attribute
    {
    }
}