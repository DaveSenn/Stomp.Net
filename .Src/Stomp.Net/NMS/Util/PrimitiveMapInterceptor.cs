#region Usings

using System;
using System.Collections;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     This class provides a mechanism to intercept calls to a IPrimitiveMap
    ///     instance and perform validation, handle type conversion, or some other
    ///     function necessary to use the PrimitiveMap in a Message or other NMS
    ///     object.
    ///     Be default this class enforces the standard conversion policy for primitive
    ///     types in NMS shown in the table below:
    ///     |        | boolean byte short char int long float double String byte[]
    ///     |----------------------------------------------------------------------
    ///     |boolean |    X                                            X
    ///     |byte    |          X     X         X   X                  X
    ///     |short   |                X         X   X                  X
    ///     |char    |                     X                           X
    ///     |int     |                          X   X                  X
    ///     |long    |                              X                  X
    ///     |float   |                                    X     X      X
    ///     |double  |                                          X      X
    ///     |String  |    X     X     X         X   X     X     X      X
    ///     |byte[]  |                                                       X
    ///     |----------------------------------------------------------------------
    /// </summary>
    public class PrimitiveMapInterceptor : IPrimitiveMap
    {
        #region Fields

        protected IMessage message;
        protected IPrimitiveMap properties;

        #endregion

        #region Properties

        public Boolean ReadOnly { get; set; }

        public Boolean AllowByteArrays { get; set; } = true;

        #endregion

        #region Ctor

        public PrimitiveMapInterceptor( IMessage message, IPrimitiveMap properties )
        {
            this.message = message;
            this.properties = properties;
        }

        public PrimitiveMapInterceptor( IMessage message, IPrimitiveMap properties, Boolean readOnly )
        {
            this.message = message;
            this.properties = properties;
            ReadOnly = readOnly;
        }

        public PrimitiveMapInterceptor( IMessage message, IPrimitiveMap properties, Boolean readOnly, Boolean allowByteArrays )
        {
            this.message = message;
            this.properties = properties;
            ReadOnly = readOnly;
            AllowByteArrays = allowByteArrays;
        }

        #endregion

        protected virtual void FailIfReadOnly()
        {
            if ( ReadOnly )
                throw new MessageNotWriteableException( "Properties are in Read-Only mode." );
        }

        protected virtual Object GetObjectProperty( String name ) => properties[name];

        protected virtual void SetObjectProperty( String name, Object value )
        {
            FailIfReadOnly();

            try
            {
                if ( !AllowByteArrays && value is Byte[] )
                    throw new NotSupportedException( "Byte Arrays not allowed in this PrimitiveMap" );

                properties[name] = value;
            }
            catch ( Exception ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        #region IPrimitiveMap Members

        public void Clear()
        {
            FailIfReadOnly();
            properties.Clear();
        }

        public Boolean Contains( Object key ) => properties.Contains( key );

        public void Remove( Object key )
        {
            FailIfReadOnly();
            properties.Remove( key );
        }

        public Int32 Count
        {
            get { return properties.Count; }
        }

        public ICollection Keys
        {
            get { return properties.Keys; }
        }

        public ICollection Values
        {
            get { return properties.Values; }
        }

        public Object this[ String key ]
        {
            get { return GetObjectProperty( key ); }
            set { SetObjectProperty( key, value ); }
        }

        public String GetString( String key )
        {
            var value = GetObjectProperty( key );

            if ( value == null )
                return null;
            if ( value is IList || value is IDictionary )
                throw new MessageFormatException( " cannot read a boolean from " + value.GetType()
                                                                                        .Name );

            return value.ToString();
        }

        public void SetString( String key, String value ) => SetObjectProperty( key, value );

        public Boolean GetBool( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Boolean )
                    return (Boolean) value;
                if ( value is String )
                    return ( (String) value ).ToLower() == "true";
                throw new MessageFormatException( " cannot read a boolean from " + value.GetType()
                                                                                        .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetBool( String key, Boolean value ) => SetObjectProperty( key, value );

        public Byte GetByte( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Byte )
                    return (Byte) value;
                if ( value is String )
                    return Convert.ToByte( value );
                throw new MessageFormatException( " cannot read a byte from " + value.GetType()
                                                                                     .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetByte( String key, Byte value ) => SetObjectProperty( key, value );

        public Char GetChar( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Char )
                    return (Char) value;
                if ( value is String )
                {
                    var svalue = value as String;
                    if ( svalue.Length == 1 )
                        return svalue.ToCharArray()[0];
                }

                throw new MessageFormatException( " cannot read a char from " + value.GetType()
                                                                                     .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetChar( String key, Char value ) => SetObjectProperty( key, value );

        public Int16 GetShort( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Int16 )
                    return (Int16) value;
                if ( value is Byte || value is String )
                    return Convert.ToInt16( value );
                throw new MessageFormatException( " cannot read a short from " + value.GetType()
                                                                                      .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetShort( String key, Int16 value ) => SetObjectProperty( key, value );

        public Int32 GetInt( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Int32 )
                    return (Int32) value;
                if ( value is Int16 || value is Byte || value is String )
                    return Convert.ToInt32( value );
                throw new MessageFormatException( " cannot read a int from " + value.GetType()
                                                                                    .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetInt( String key, Int32 value ) => SetObjectProperty( key, value );

        public Int64 GetLong( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Int64 )
                    return (Int64) value;
                if ( value is Int32 || value is Int16 || value is Byte || value is String )
                    return Convert.ToInt64( value );
                throw new MessageFormatException( " cannot read a long from " + value.GetType()
                                                                                     .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetLong( String key, Int64 value ) => SetObjectProperty( key, value );

        public Single GetFloat( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Single )
                    return (Single) value;
                if ( value is String )
                    return Convert.ToSingle( value );
                throw new MessageFormatException( " cannot read a float from " + value.GetType()
                                                                                      .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetFloat( String key, Single value ) => SetObjectProperty( key, value );

        public Double GetDouble( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Double )
                    return (Double) value;
                if ( value is Single || value is String )
                    return Convert.ToDouble( value );
                throw new MessageFormatException( " cannot read a double from " + value.GetType()
                                                                                       .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public void SetDouble( String key, Double value ) => SetObjectProperty( key, value );

        public void SetBytes( String key, Byte[] value ) => SetBytes( key, value, 0, value.Length );

        public void SetBytes( String key, Byte[] value, Int32 offset, Int32 length )
        {
            var copy = new Byte[length];
            Array.Copy( value, offset, copy, 0, length );
            SetObjectProperty( key, value );
        }

        public Byte[] GetBytes( String key )
        {
            var value = GetObjectProperty( key );

            try
            {
                if ( value is Byte[] )
                    return (Byte[]) value;
                throw new MessageFormatException( " cannot read a byte[] from " + value.GetType()
                                                                                       .Name );
            }
            catch ( FormatException ex )
            {
                throw NMSExceptionSupport.CreateMessageFormatException( ex );
            }
        }

        public IList GetList( String key ) => (IList) GetObjectProperty( key );

        public void SetList( String key, IList list ) => SetObjectProperty( key, list );

        public IDictionary GetDictionary( String key ) => (IDictionary) GetObjectProperty( key );

        public void SetDictionary( String key, IDictionary dictionary ) => SetObjectProperty( key, dictionary );

        #endregion
    }
}