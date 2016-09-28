﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates the type member or parameter of some type, that should be used instead of all other ways
    ///     to get the value that type. This annotation is useful when you have some "context" value evaluated
    ///     and stored somewhere, meaning that all other ways to get this value must be consolidated with existing one.
    /// </summary>
    /// <example>
    ///     <code>
    /// class Foo {
    ///   [ProvidesContext] IBarService _barService = ...;
    /// 
    ///   void ProcessNode(INode node) {
    ///     DoSomething(node, node.GetGlobalServices().Bar);
    ///     //              ^ Warning: use value of '_barService' field
    ///   }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(
         AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter |
         AttributeTargets.Method )]
    internal sealed class ProvidesContextAttribute : Attribute
    {
    }
}