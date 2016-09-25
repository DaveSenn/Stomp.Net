

#region Usings

using System;
using System.Collections;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a Map of primitive types where the keys are all string instances
    ///     and the values are strings or numbers.
    /// </summary>
    public interface IPrimitiveMap
    {
        #region Properties

        Int32 Count { get; }

        ICollection Keys { get; }

        ICollection Values { get; }

        Object this[ String key ] { get; set; }

        #endregion

        void Clear();

        Boolean Contains( Object key );

        Boolean GetBool( String key );

        Byte GetByte( String key );
        Byte[] GetBytes( String key );

        Char GetChar( String key );

        IDictionary GetDictionary( String key );

        Double GetDouble( String key );

        Single GetFloat( String key );

        Int32 GetInt( String key );

        IList GetList( String key );

        Int64 GetLong( String key );

        Int16 GetShort( String key );

        String GetString( String key );

        void Remove( Object key );
        void SetBool( String key, Boolean value );
        void SetByte( String key, Byte value );

        void SetBytes( String key, Byte[] value );
        void SetBytes( String key, Byte[] value, Int32 offset, Int32 length );
        void SetChar( String key, Char value );
        void SetDictionary( String key, IDictionary dictionary );
        void SetDouble( String key, Double value );
        void SetFloat( String key, Single value );
        void SetInt( String key, Int32 value );
        void SetList( String key, IList list );
        void SetLong( String key, Int64 value );
        void SetShort( String key, Int16 value );
        void SetString( String key, String value );
    }
}