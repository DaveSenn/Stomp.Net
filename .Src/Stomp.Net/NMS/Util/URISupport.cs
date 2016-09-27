#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Extend;
using JetBrains.Annotations;
using System.Web;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     Class to provide support for Uri query parameters which uses .Net reflection
    ///     to identify and set properties.
    /// </summary>
    public class UriSupport
    {
        #region Properties

        private static Dictionary<String, String> EmptyMap
        {
            get { return new Dictionary<String, String>(); }
        }

        #endregion

        /// <summary>
        ///     Given a string that could be a Composite Uri that uses syntax not compatible
        ///     with the .NET Uri class such as an ActiveMQ failover Uri formatted as
        ///     "failover://(tcp://localhost:61616)", the initial '://' must be changed
        ///     to ':(' so that the Uri class doesn't attempt to parse the '(tcp:' as
        ///     the Uri's Authority as that is not a valid host name.
        /// </summary>
        /// <param name="uriString">
        ///     A string that could be a Composite Uri that uses syntax not compatible
        ///     with the .NET Uri class
        /// </param>
        public static Uri CreateCompatibleUri( String uriString )
        {
            var sanitized = uriString.Replace( "://(", ":(" );
            return new Uri( sanitized );
        }

        public static Dictionary<String, String> GetProperties( [NotNull] Dictionary<String, String> props, String prefix )
        {
            props.ThrowIfNull( nameof( props ) );

            var result = new Dictionary<String, String>();
            foreach ( var key in props.Keys )
                if ( key.StartsWith( prefix, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    var bareKey = key.Substring( prefix.Length );
                    var value = props[key];
                    result[bareKey] = value;
                }

            return result;
        }

        /// <summary>
        ///     Sets the public properties of a target object using a string map.
        ///     This method uses .Net reflection to identify public properties of
        ///     the target object matching the keys from the passed map.
        /// </summary>
        /// <param name="target">The object whose properties will be set.</param>
        /// <param name="map">Map of key/value pairs.</param>
        public static void SetProperties( Object target, Dictionary<String, String> map )
        {
            var type = target.GetType();

            foreach ( var key in map.Keys )
            {
                var property = type.GetProperty( key,
                                             BindingFlags.FlattenHierarchy
                                             | BindingFlags.Public
                                             | BindingFlags.Instance
                                             | BindingFlags.IgnoreCase );

                if ( null != property )
                {
                    property.SetValue( target, Convert.ChangeType( map[key], property.PropertyType, CultureInfo.InvariantCulture ), null );
                }
                else
                {
                    var field = type.GetField( key,
                                               BindingFlags.FlattenHierarchy
                                               | BindingFlags.Public
                                               | BindingFlags.Instance
                                               | BindingFlags.IgnoreCase );
                    if ( null != field )
                        field.SetValue( target, Convert.ChangeType( map[key], field.FieldType, CultureInfo.InvariantCulture ) );
                    else
                        throw new NMSException( String.Format( "No such property or field: {0} on class: {1}",
                                                               key,
                                                               target.GetType()
                                                                     .Name ) );
                }
            }
        }

        #region Refactored

        /// <summary>
        ///     Removes all key/value pairs from the given dictionary, where the key starts with the given prefix.
        /// </summary>
        /// <param name="properties">A collection of key/value pairs.</param>
        /// <param name="prefix">The key-prefix to search for.</param>
        /// <returns>Returns the removed values.</returns>
        [NotNull]
        public static Dictionary<String, String> ExtractProperties( [NotNull] Dictionary<String, String> properties, [NotNull] String prefix )
        {
            properties.ThrowIfNull( nameof( properties ) );
            prefix.ThrowIfNull( nameof( prefix ) );

            var result = new Dictionary<String, String>();

            foreach ( var key in properties.Keys )
                if ( key.StartsWith( prefix, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    var value = properties[key];
                    result[key] = value;
                }

            foreach ( var match in result )
                properties.Remove( match.Key );

            return result;
        }

        /// <summary>
        ///     Parse a Uri query string of the form ?x=y&amp;z=0
        ///     into a map of name/value pairs.
        /// </summary>
        /// <param name="query">
        ///     The query string to parse. This string should not contain
        ///     Uri escape characters.
        /// </param>
        [NotNull]
        public static Dictionary<String, String> ParseQuery( [CanBeNull] String query )
        {
            var parameters = new Dictionary<String, String>();

            // Check if any parameters are specified
            if ( query.IsEmpty() )
                return parameters;

            // Strip the initial "?"
            // ReSharper disable once PossibleNullReferenceException
            if ( query.StartsWith( "?", StringComparison.Ordinal ) )
                query = query.Substring( 1 );

            // Split the query into parameters
            var parameterStrings = query.Split( new[] { '&' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var nameValue in parameterStrings.Select( x => x.Split( '=' ) ) )
            {
                // Check for valid key/value pair
                if ( nameValue.Length != 2 )
                    throw new NMSException( $"Invalid URI parameters: '{query}'." );

                var decodedKey = HttpUtility.UrlDecode( nameValue[0] );
                if ( decodedKey == null )
                    throw new NMSException( $"Invalid URI parameter key: '{nameValue[0]}' in '{query}'." );
                parameters[decodedKey] = HttpUtility.UrlDecode( nameValue[1] );
            }

            return parameters;
        }

        /// <summary>
        ///     Sets the public properties of a target object using a string map.
        ///     This method uses .Net reflection to identify public properties of
        ///     the target object matching the keys from the passed map.
        /// </summary>
        /// <param name="target">The object whose properties will be set.</param>
        /// <param name="map">Map of key/value pairs.</param>
        /// <param name="prefix">
        ///     Key value prefix. This is perpended to the property name
        ///     before searching for a matching key value.
        /// </param>
        public static void SetProperties( [NotNull] Object target, [NotNull] Dictionary<String, String> map, [NotNull] String prefix )
        {
            target.ThrowIfNull( nameof( target ) );
            map.ThrowIfNull( nameof( map ) );
            prefix.ThrowIfNull( nameof( prefix ) );

            var type = target.GetType();
            var matches = new List<String>();

            foreach ( var key in map.Keys )
            {
                // CHeck if it matches the given prefix
                if ( !key.StartsWith( prefix, StringComparison.InvariantCultureIgnoreCase ) )
                    continue;

                var bareKey = key.Substring( prefix.Length );
                var prop = type.GetProperty( bareKey,
                                             BindingFlags.FlattenHierarchy
                                             | BindingFlags.Public
                                             | BindingFlags.Instance
                                             | BindingFlags.IgnoreCase );

                if ( null != prop )
                {
                    prop.SetValue( target, Convert.ChangeType( map[key], prop.PropertyType, CultureInfo.InvariantCulture ), null );
                }
                else
                {
                    var field = type.GetField( bareKey,
                                               BindingFlags.FlattenHierarchy
                                               | BindingFlags.Public
                                               | BindingFlags.Instance
                                               | BindingFlags.IgnoreCase );
                    if ( null != field )
                        field.SetValue( target, Convert.ChangeType( map[key], field.FieldType, CultureInfo.InvariantCulture ) );
                    else
                        throw new NMSException( String.Format( "No such property or field: {0} on class: {1}",
                                                               bareKey,
                                                               target.GetType()
                                                                     .Name ) );
                }

                // store for later removal.
                matches.Add( key );
            }

            // Remove all the properties we set so they are used again later.
            foreach ( var match in matches )
                map.Remove( match );
        }

        #endregion
    }
}