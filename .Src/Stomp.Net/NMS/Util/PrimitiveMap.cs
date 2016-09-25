/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

#region Usings

using System;
using System.Collections;
using System.IO;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     A default implementation of IPrimitiveMap
    /// </summary>
    public class PrimitiveMap : IPrimitiveMap
    {
        #region Constants

        public const Byte BIG_STRING_TYPE = 13;
        public const Byte BOOLEAN_TYPE = 1;
        public const Byte BYTE_ARRAY_TYPE = 10;
        public const Byte BYTE_TYPE = 2;
        public const Byte CHAR_TYPE = 3;
        public const Byte DOUBLE_TYPE = 7;
        public const Byte FLOAT_TYPE = 8;
        public const Byte INTEGER_TYPE = 5;
        public const Byte LIST_TYPE = 12;
        public const Byte LONG_TYPE = 6;
        public const Byte MAP_TYPE = 11;
        public const Byte NULL = 0;
        public const Byte SHORT_TYPE = 4;
        public const Byte STRING_TYPE = 9;

        #endregion

        #region Fields

        private IDictionary dictionary = Hashtable.Synchronized( new Hashtable() );

        #endregion

        public void Clear()
        {
            dictionary.Clear();
        }

        public Boolean Contains( Object key )
        {
            return dictionary.Contains( key );
        }

        public Int32 Count
        {
            get { return dictionary.Count; }
        }

        public Boolean GetBool( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Boolean) );
            return (Boolean) value;
        }

        public Byte GetByte( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Byte) );
            return (Byte) value;
        }

        public Byte[] GetBytes( String key )
        {
            var value = GetValue( key );
            if ( value != null && !( value is Byte[] ) )
                throw new NMSException( "Property: " + key + " is not an byte[] but is: " + value );
            return (Byte[]) value;
        }

        public Char GetChar( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Char) );
            return (Char) value;
        }

        public IDictionary GetDictionary( String key )
        {
            var value = GetValue( key );
            if ( value != null && !( value is IDictionary ) )
                throw new NMSException( "Property: " + key + " is not an IDictionary but is: " + value );
            return (IDictionary) value;
        }

        public Double GetDouble( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Double) );
            return (Double) value;
        }

        public Single GetFloat( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Single) );
            return (Single) value;
        }

        public Int32 GetInt( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Int32) );
            return (Int32) value;
        }

        public IList GetList( String key )
        {
            var value = GetValue( key );
            if ( value != null && !( value is IList ) )
                throw new NMSException( "Property: " + key + " is not an IList but is: " + value );
            return (IList) value;
        }

        public Int64 GetLong( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Int64) );
            return (Int64) value;
        }

        public Int16 GetShort( String key )
        {
            var value = GetValue( key );
            CheckValueType( value, typeof(Int16) );
            return (Int16) value;
        }

        public String GetString( String key )
        {
            var value = GetValue( key );
            if ( value == null )
                return null;
            CheckValueType( value, typeof(String) );
            return (String) value;
        }

        public Object this[ String key ]
        {
            get { return GetValue( key ); }
            set
            {
                CheckValidType( value );
                SetValue( key, value );
            }
        }

        public ICollection Keys
        {
            get
            {
                lock ( dictionary.SyncRoot )
                    return new ArrayList( dictionary.Keys );
            }
        }

        public void Remove( Object key )
        {
            dictionary.Remove( key );
        }

        public void SetBool( String key, Boolean value )
        {
            SetValue( key, value );
        }

        public void SetByte( String key, Byte value )
        {
            SetValue( key, value );
        }

        public void SetBytes( String key, Byte[] value )
        {
            SetBytes( key, value, 0, value.Length );
        }

        public void SetBytes( String key, Byte[] value, Int32 offset, Int32 length )
        {
            var copy = new Byte[length];
            Array.Copy( value, offset, copy, 0, length );
            SetValue( key, copy );
        }

        public void SetChar( String key, Char value )
        {
            SetValue( key, value );
        }

        public void SetDictionary( String key, IDictionary value )
        {
            SetValue( key, value );
        }

        public void SetDouble( String key, Double value )
        {
            SetValue( key, value );
        }

        public void SetFloat( String key, Single value )
        {
            SetValue( key, value );
        }

        public void SetInt( String key, Int32 value )
        {
            SetValue( key, value );
        }

        public void SetList( String key, IList value )
        {
            SetValue( key, value );
        }

        public void SetLong( String key, Int64 value )
        {
            SetValue( key, value );
        }

        public void SetShort( String key, Int16 value )
        {
            SetValue( key, value );
        }

        public void SetString( String key, String value )
        {
            SetValue( key, value );
        }

        public ICollection Values
        {
            get
            {
                lock ( dictionary.SyncRoot )
                    return new ArrayList( dictionary.Values );
            }
        }

        public Byte[] Marshal()
        {
            lock ( dictionary.SyncRoot )
                return MarshalPrimitiveMap( dictionary );
        }

        /// <summary>
        ///     Marshals a PrimitiveMap directly to a Stream object.  This
        ///     allows a client to write a PrimitiveMap in a compressed or
        ///     otherwise encoded form without this class needing to know
        ///     about it.
        /// </summary>
        /// <param name="destination">
        ///     A <see cref="Stream" />
        /// </param>
        public void Marshal( Stream destination )
        {
            lock ( dictionary.SyncRoot )
                MarshalPrimitiveMap( dictionary, destination );
        }

        public static void MarshalPrimitive( BinaryWriter dataOut, Object value )
        {
            if ( value == null )
            {
                dataOut.Write( NULL );
            }
            else if ( value is Boolean )
            {
                dataOut.Write( BOOLEAN_TYPE );
                dataOut.Write( (Boolean) value );
            }
            else if ( value is Byte )
            {
                dataOut.Write( BYTE_TYPE );
                dataOut.Write( (Byte) value );
            }
            else if ( value is Char )
            {
                dataOut.Write( CHAR_TYPE );
                dataOut.Write( (Char) value );
            }
            else if ( value is Int16 )
            {
                dataOut.Write( SHORT_TYPE );
                dataOut.Write( (Int16) value );
            }
            else if ( value is Int32 )
            {
                dataOut.Write( INTEGER_TYPE );
                dataOut.Write( (Int32) value );
            }
            else if ( value is Int64 )
            {
                dataOut.Write( LONG_TYPE );
                dataOut.Write( (Int64) value );
            }
            else if ( value is Single )
            {
                dataOut.Write( FLOAT_TYPE );
                dataOut.Write( (Single) value );
            }
            else if ( value is Double )
            {
                dataOut.Write( DOUBLE_TYPE );
                dataOut.Write( (Double) value );
            }
            else if ( value is Byte[] )
            {
                var data = (Byte[]) value;
                dataOut.Write( BYTE_ARRAY_TYPE );
                dataOut.Write( data.Length );
                dataOut.Write( data );
            }
            else if ( value is String )
            {
                var s = (String) value;
                // is the string big??
                if ( s.Length > 8191 )
                {
                    dataOut.Write( BIG_STRING_TYPE );
                    ( (EndianBinaryWriter) dataOut ).WriteString32( s );
                }
                else
                {
                    dataOut.Write( STRING_TYPE );
                    ( (EndianBinaryWriter) dataOut ).WriteString16( s );
                }
            }
            else if ( value is IDictionary )
            {
                dataOut.Write( MAP_TYPE );
                MarshalPrimitiveMap( (IDictionary) value, dataOut );
            }
            else if ( value is IList )
            {
                dataOut.Write( LIST_TYPE );
                MarshalPrimitiveList( (IList) value, dataOut );
            }
            else
            {
                throw new IOException( "Object is not a primitive: " + value );
            }
        }

        public static void MarshalPrimitiveList( IList list, BinaryWriter dataOut )
        {
            dataOut.Write( list.Count );
            foreach ( var element in list )
                MarshalPrimitive( dataOut, element );
        }

        /// <summary>
        ///     Marshals the primitive type map to a byte array
        /// </summary>
        public static Byte[] MarshalPrimitiveMap( IDictionary map )
        {
            if ( map == null )
                return null;

            var memoryStream = new MemoryStream();
            lock ( map.SyncRoot )
                MarshalPrimitiveMap( map, new EndianBinaryWriter( memoryStream ) );

            return memoryStream.ToArray();
        }

        public static void MarshalPrimitiveMap( IDictionary map, Stream stream )
        {
            if ( map != null )
                lock ( map.SyncRoot )
                    MarshalPrimitiveMap( map, new EndianBinaryWriter( stream ) );
        }

        public static void MarshalPrimitiveMap( IDictionary map, BinaryWriter dataOut )
        {
            if ( map == null )
                dataOut.Write( -1 );
            else
                lock ( map.SyncRoot )
                {
                    dataOut.Write( map.Count );
                    foreach ( DictionaryEntry entry in map )
                    {
                        var name = (String) entry.Key;
                        dataOut.Write( name );
                        var value = entry.Value;
                        MarshalPrimitive( dataOut, value );
                    }
                }
        }

        /// <summary>
        ///     Method ToString
        /// </summary>
        /// <returns>A string</returns>
        public override String ToString()
        {
            var s = "{";
            var first = true;
            lock ( dictionary.SyncRoot )
                foreach ( DictionaryEntry entry in dictionary )
                {
                    if ( !first )
                        s += ", ";
                    first = false;
                    var name = (String) entry.Key;
                    var value = entry.Value;
                    s += name + "=" + value;
                }
            s += "}";
            return s;
        }

        /// <summary>
        ///     Unmarshalls the map from the given data or if the data is null just
        ///     return an empty map
        /// </summary>
        public static PrimitiveMap Unmarshal( Byte[] data )
        {
            var answer = new PrimitiveMap();
            answer.dictionary = UnmarshalPrimitiveMap( data );
            return answer;
        }

        /// <summary>
        ///     Unmarshals a PrimitiveMap directly from a Stream object.  This
        ///     allows for clients to read PrimitiveMaps from Compressed or other
        ///     wise encoded streams without this class needing to know about it.
        /// </summary>
        /// <param name="source">
        ///     A <see cref="Stream" />
        /// </param>
        /// <returns>
        ///     A <see cref="PrimitiveMap" />
        /// </returns>
        public static PrimitiveMap Unmarshal( Stream source )
        {
            var answer = new PrimitiveMap();
            answer.dictionary = UnmarshalPrimitiveMap( source );
            return answer;
        }

        public static Object UnmarshalPrimitive( BinaryReader dataIn )
        {
            Object value = null;
            var type = dataIn.ReadByte();
            switch ( type )
            {
                case NULL:
                    value = null;
                    break;
                case BYTE_TYPE:
                    value = dataIn.ReadByte();
                    break;
                case BOOLEAN_TYPE:
                    value = dataIn.ReadBoolean();
                    break;
                case CHAR_TYPE:
                    value = dataIn.ReadChar();
                    break;
                case SHORT_TYPE:
                    value = dataIn.ReadInt16();
                    break;
                case INTEGER_TYPE:
                    value = dataIn.ReadInt32();
                    break;
                case LONG_TYPE:
                    value = dataIn.ReadInt64();
                    break;
                case FLOAT_TYPE:
                    value = dataIn.ReadSingle();
                    break;
                case DOUBLE_TYPE:
                    value = dataIn.ReadDouble();
                    break;
                case BYTE_ARRAY_TYPE:
                    var size = dataIn.ReadInt32();
                    var data = new Byte[size];
                    dataIn.Read( data, 0, size );
                    value = data;
                    break;
                case STRING_TYPE:
                    value = ( (EndianBinaryReader) dataIn ).ReadString16();
                    break;
                case BIG_STRING_TYPE:
                    value = ( (EndianBinaryReader) dataIn ).ReadString32();
                    break;
                case MAP_TYPE:
                    value = UnmarshalPrimitiveMap( dataIn );
                    break;
                case LIST_TYPE:
                    value = UnmarshalPrimitiveList( dataIn );
                    break;

                default:
                    throw new Exception( "Unsupported data type: " + type );
            }
            return value;
        }

        public static IList UnmarshalPrimitiveList( BinaryReader dataIn )
        {
            var size = dataIn.ReadInt32();
            IList answer = new ArrayList( size );
            while ( size-- > 0 )
                answer.Add( UnmarshalPrimitive( dataIn ) );

            return answer;
        }

        /// <summary>
        ///     Unmarshals the primitive type map from the given byte array
        /// </summary>
        public static IDictionary UnmarshalPrimitiveMap( Byte[] data )
        {
            if ( data == null )
                return new Hashtable();
            return UnmarshalPrimitiveMap( new EndianBinaryReader( new MemoryStream( data ) ) );
        }

        public static IDictionary UnmarshalPrimitiveMap( Stream source )
        {
            return UnmarshalPrimitiveMap( new EndianBinaryReader( source ) );
        }

        public static IDictionary UnmarshalPrimitiveMap( BinaryReader dataIn )
        {
            var size = dataIn.ReadInt32();
            if ( size < 0 )
                return null;

            IDictionary answer = new Hashtable( size );
            for ( var i = 0; i < size; i++ )
            {
                var name = dataIn.ReadString();
                answer[name] = UnmarshalPrimitive( dataIn );
            }

            return answer;
        }

        protected virtual void CheckValidType( Object value )
        {
            if ( value != null && !( value is IList ) && !( value is IDictionary ) )
            {
                var type = value.GetType();

                if ( type.IsInstanceOfType( typeof(Object) ) ||
                     ( !type.IsPrimitive && !type.IsValueType && !type.IsAssignableFrom( typeof(String) ) ) )
                    throw new NMSException( "Invalid type: " + type.Name + " for value: " + value );
            }
        }

        protected virtual void CheckValueType( Object value, Type type )
        {
            if ( !type.IsInstanceOfType( value ) )
                throw new NMSException( "Expected type: " + type.Name + " but was: " + value );
        }

        protected virtual Object GetValue( String key )
        {
            return dictionary[key];
        }

        protected virtual void SetValue( String key, Object value )
        {
            dictionary[key] = value;
        }
    }
}