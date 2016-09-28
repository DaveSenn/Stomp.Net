#region Usings

using System;

#endregion

namespace JetBrains.Annotations
{
    /// <summary>
    ///     Indicates how method, constructor invocation or property access
    ///     over collection type affects content of the collection.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property )]
    internal sealed class CollectionAccessAttribute : Attribute
    {
        #region Properties

        public CollectionAccessType CollectionAccessType { get; private set; }

        #endregion

        #region Ctor

        public CollectionAccessAttribute( CollectionAccessType collectionAccessType )
        {
            CollectionAccessType = collectionAccessType;
        }

        #endregion
    }
}