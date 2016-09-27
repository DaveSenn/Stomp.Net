#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class AtomicCollection<TValue>
        where TValue : class
    {
        #region Fields

        private readonly ArrayList _collection = new ArrayList();

        #endregion

        #region Properties

        public Int32 Count
        {
            get
            {
                lock ( _collection.SyncRoot )
                    return _collection.Count;
            }
        }

        public Boolean IsReadOnly
        {
            get { return false; }
        }

        public TValue this[ Int32 index ]
        {
            get
            {
                TValue ret;
                lock ( _collection.SyncRoot )
                    ret = (TValue) _collection[index];
                return ret;
            }
            set
            {
                lock ( _collection.SyncRoot )
                    _collection[index] = value;
            }
        }

        #endregion

        #region Ctor

        public AtomicCollection()
        {
        }

        public AtomicCollection( ICollection c )
        {
            lock ( c.SyncRoot )
                foreach ( var obj in c )
                    _collection.Add( obj );
        }

        #endregion

        public Int32 Add( TValue v )
        {
            lock ( _collection.SyncRoot )
                return _collection.Add( v );
        }

        public void Clear()
        {
            lock ( _collection.SyncRoot )
                _collection.Clear();
        }

        public Boolean Contains( TValue v )
        {
            lock ( _collection.SyncRoot )
                return _collection.Contains( v );
        }

        public void CopyTo( TValue[] a, Int32 index )
        {
            lock ( _collection.SyncRoot )
                _collection.CopyTo( a, index );
        }

        public IEnumerator GetEnumerator()
        {
            lock ( _collection.SyncRoot )
                return _collection.GetEnumerator();
        }

#if !NETCF
        public IEnumerator GetEnumerator( Int32 index, Int32 count )
        {
            lock ( _collection.SyncRoot )
                return _collection.GetEnumerator( index, count );
        }
#endif

        public void Remove( TValue v )
        {
            lock ( _collection.SyncRoot )
                _collection.Remove( v );
        }

        public void RemoveAt( Int32 index )
        {
            lock ( _collection.SyncRoot )
                _collection.RemoveAt( index );
        }
    }

    public class AtomicDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        #region Fields

        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        #endregion

        #region Properties

        public TValue this[ TKey key ]
        {
            get
            {
                TValue ret;
                lock ( ( (ICollection) _dictionary ).SyncRoot )
                    ret = _dictionary[key];
                return ret;
            }
            set
            {
                lock ( ( (ICollection) _dictionary ).SyncRoot )
                    _dictionary[key] = value;
            }
        }

        public AtomicCollection<TKey> Keys
        {
            get
            {
                lock ( ( (ICollection) _dictionary ).SyncRoot )
                    return new AtomicCollection<TKey>( _dictionary.Keys );
            }
        }

        public AtomicCollection<TValue> Values
        {
            get
            {
                lock ( ( (ICollection) _dictionary ).SyncRoot )
                    return new AtomicCollection<TValue>( _dictionary.Values );
            }
        }

        #endregion

        public void Add( TKey k, TValue v )
        {
            lock ( ( (ICollection) _dictionary ).SyncRoot )
                _dictionary.Add( k, v );
        }

        public void Clear() => _dictionary.Clear();

        public Boolean ContainsKey( TKey k )
        {
            lock ( ( (ICollection) _dictionary ).SyncRoot )
                return _dictionary.ContainsKey( k );
        }

        public Boolean ContainsValue( TValue v )
        {
            lock ( ( (ICollection) _dictionary ).SyncRoot )
                return _dictionary.ContainsValue( v );
        }

        public Boolean Remove( TKey v )
        {
            lock ( ( (ICollection) _dictionary ).SyncRoot )
                return _dictionary.Remove( v );
        }

        public Boolean TryGetValue( TKey key, out TValue val )
        {
            lock ( ( (ICollection) _dictionary ).SyncRoot )
                return _dictionary.TryGetValue( key, out val );
        }
    }
}