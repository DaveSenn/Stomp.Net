

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

        public Boolean CompareAndSet( T expected, T newValue )
        {
            lock ( this )
            {
                if ( 0 == atomicValue.CompareTo( expected ) )
                {
                    atomicValue = newValue;
                    return true;
                }

                return false;
            }
        }
    }
}