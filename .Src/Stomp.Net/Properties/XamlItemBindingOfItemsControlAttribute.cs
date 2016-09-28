﻿#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     XAML attribute. Indicates the property of some <c>BindingBase</c>-derived type, that
    ///     is used to bind some item of <c>ItemsControl</c>-derived type. This annotation will
    ///     enable the <c>DataContext</c> type resolve for XAML bindings for such properties.
    /// </summary>
    /// <remarks>
    ///     Property should have the tree ancestor of the <c>ItemsControl</c> type or
    ///     marked with the <see cref="XamlItemsControlAttribute" /> attribute.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Property )]
    internal sealed class XamlItemBindingOfItemsControlAttribute : Attribute
    {
    }
}