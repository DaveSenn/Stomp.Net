#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     When applied to a target attribute, specifies a requirement for any type marked
    ///     with the target attribute to implement or inherit specific type or types.
    /// </summary>
    /// <example>
    ///     <code>
    /// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
    /// class ComponentAttribute : Attribute { }
    /// 
    /// [Component] // ComponentAttribute requires implementing IComponent interface
    /// class MyComponent : IComponent { }
    /// </code>
    /// </example>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    [BaseTypeRequired( typeof(Attribute) )]
    internal sealed class BaseTypeRequiredAttribute : Attribute
    {
        #region Properties

        [NotNull]
        public Type BaseType { get; private set; }

        #endregion

        #region Ctor

        public BaseTypeRequiredAttribute( [NotNull] Type baseType )
        {
            BaseType = baseType;
        }

        #endregion
    }
}