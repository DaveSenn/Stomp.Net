﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Can be appplied to symbols of types derived from IEnumerable as well as to symbols of Task
    ///     and Lazy classes to indicate that the value of a collection item, of the Task.Result property
    ///     or of the Lazy.Value property can never be null.
    /// </summary>
    [AttributeUsage(
         AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
         AttributeTargets.Delegate | AttributeTargets.Field )]
    internal sealed class ItemNotNullAttribute : Attribute
    {
    }
}