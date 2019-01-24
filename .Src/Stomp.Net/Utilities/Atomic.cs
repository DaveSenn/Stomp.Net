#region Usings

using System;

#endregion

namespace Stomp.Net.Utilities
{
    public class Atomic<T> : AtomicReference<T> where T : IComparable
    {
        /// <summary>
        ///     Compares the current value with the expected value.
        ///     If it matches the value will be updated to <paramref name="newValue" />.
        /// </summary>
        /// <param name="expected">The currently expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>Returns a value of true if the current value matches the expected value.</returns>
        public Boolean CompareAndSet( T expected, T newValue )
        {
            lock ( this )
            {
                if ( 0 != AtomicValue.CompareTo( expected ) )
                    return false;

                AtomicValue = newValue;
                return true;
            }
        }

        #region Ctor

        public Atomic()
        {
        }

        public Atomic( T defaultValue )
            : base( defaultValue )
        {
        }

        #endregion
    }
}