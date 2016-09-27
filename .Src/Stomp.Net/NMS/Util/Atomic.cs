#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    public class AtomicReference<T>
    {
        #region Fields

        protected T atomicValue;

        #endregion

        #region Properties

        public T Value
        {
            get
            {
                lock ( this )
                    return atomicValue;
            }
            set
            {
                lock ( this )
                    atomicValue = value;
            }
        }

        #endregion

        #region Ctor

        public AtomicReference()
        {
            atomicValue = default(T);
        }

        public AtomicReference( T defaultValue )
        {
            atomicValue = defaultValue;
        }

        #endregion

        public T GetAndSet( T value )
        {
            lock ( this )
            {
                var ret = atomicValue;
                atomicValue = value;
                return ret;
            }
        }
    }

    public class Atomic<T> : AtomicReference<T> where T : IComparable
    {
        #region Ctor

        public Atomic()
        {
        }

        public Atomic( T defaultValue )
            : base( defaultValue )
        {
        }

        #endregion

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
                if ( 0 != atomicValue.CompareTo( expected ) )
                    return false;

                atomicValue = newValue;
                return true;
            }
        }
    }
}