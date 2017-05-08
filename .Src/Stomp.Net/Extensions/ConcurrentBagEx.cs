#region Usings

using System;
using System.Collections.Concurrent;
using Extend;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Util
{
    /// <summary>
    ///     Class containing extension methods for <see cref="ConcurrentBag{T}" />.
    /// </summary>
    public static class ConcurrentBagEx
    {
        /// <summary>
        ///     Removes all items from the given <see cref="ConcurrentBag{T}" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">concurrentBag can not be null.</exception>
        /// <typeparam name="T">The type of the items in <paramref name="concurrentBag" />.</typeparam>
        /// <param name="concurrentBag">The <see cref="ConcurrentBag{T}" /> to clear.</param>
        public static void Clear<T>( [NotNull] this ConcurrentBag<T> concurrentBag )
        {
            concurrentBag.ThrowIfNull( nameof(concurrentBag) );

            while ( !concurrentBag.IsEmpty )
                concurrentBag.TryTake( out T _ );
        }
    }
}