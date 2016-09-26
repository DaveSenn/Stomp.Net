#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Extend;
using JetBrains.Annotations;
#if !NETCF
using System.Web;

#endif

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     Class to provide support for Uri query parameters which uses .Net reflection
    ///     to identify and set properties.
    /// </summary>
    public class URISupport
    {
        #region Properties

        private static Dictionary<String, String> EmptyMap
        {
            get { return new Dictionary<String, String>(); }
        }

        #endregion

        public static Boolean CheckParenthesis( String str )
        {
            var result = true;

            if ( str != null )
            {
                var open = 0;
                var closed = 0;

                var i = 0;
                while ( ( i = str.IndexOf( '(', i ) ) >= 0 )
                {
                    i++;
                    open++;
                }

                i = 0;
                while ( ( i = str.IndexOf( ')', i ) ) >= 0 )
                {
                    i++;
                    closed++;
                }

                result = open == closed;
            }

            return result;
        }

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

        public static String CreateQueryString( Dictionary<String, String> options )
        {
            if ( options != null && options.Count > 0 )
            {
                var rc = new StringBuilder();
                var first = true;

                foreach ( var key in options.Keys )
                {
                    var value = options[key];

                    if ( first )
                        first = false;
                    else
                        rc.Append( "&" );

                    rc.Append( HttpUtility.UrlEncode( key ) );
                    rc.Append( "=" );
                    rc.Append( HttpUtility.UrlEncode( value ) );
                }

                return rc.ToString();
            }
            return "";
        }

        public static Uri CreateRemainingUri( Uri originalUri, Dictionary<String, String> parameters )
        {
            var s = CreateQueryString( parameters );

            if ( String.IsNullOrEmpty( s ) )
                s = null;

            return CreateUriWithQuery( originalUri, s );
        }

        public static Uri CreateUriWithQuery( Uri uri, String query )
        {
            if ( !String.IsNullOrEmpty( query ) && !query.StartsWith( "?" ) )
                query = "?" + query;

            if ( String.IsNullOrEmpty( uri.Query ) )
                return new Uri( uri.OriginalString + query );
            var originalUri = uri.OriginalString;

            var queryDelimPos = originalUri.LastIndexOf( '?' );
            var compositeDelimPos = originalUri.LastIndexOf( ')' );

            if ( queryDelimPos <= compositeDelimPos )
                return new Uri( originalUri + query );
            // Outer Uri has a Query or not a Composite Uri with a Query
            var strippedUri = originalUri.Substring( 0, queryDelimPos );
            return new Uri( strippedUri + query );
        }

        public static Dictionary<String, String> GetProperties( Dictionary<String, String> props, String prefix )
        {
            if ( props == null )
                throw new Exception( "Properties Object was null" );

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

        public static CompositeData ParseComposite( Uri uri )
        {
            var rc = new CompositeData();
            rc.Scheme = uri.Scheme;

            // Start with original URI
            //String ssp = uri.Authority + uri.PathAndQuery;
            var ssp = uri.OriginalString;

            ssp = StripPrefix( ssp, rc.Scheme + ":" );
            ssp = StripPrefix( ssp, "//" );

            var lastPoundPos = ssp.LastIndexOf( "#" );
            var lastParendPos = ssp.LastIndexOf( ")" );

            // Only include a Fragment that's outside any Composte sections.
            if ( lastPoundPos > lastParendPos )
            {
                rc.Fragment = ssp.Substring( lastPoundPos );
                ssp = ssp.Substring( 0, lastPoundPos );
            }

            // Ensure any embedded URIs don't have malformed authority's by changing
            // them from '://(' which would cause the .NET Uri class to attempt to validate
            // the authority as a hostname with, ':(' which is valid.
            ssp = ssp.Replace( "://(", ":(" );

            // Handle the composite components
            ParseComposite( uri, rc, ssp );
            return rc;
        }

        public static Dictionary<String, String> ParseParameters( Uri uri ) => uri.Query == null
            ? new Dictionary<String, String>()
            : ParseQuery( StripPrefix( uri.Query, "?" ) );

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
                var prop = type.GetProperty( key,
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

        public static String StripPrefix( String value, String prefix )
            => value.StartsWith( prefix, StringComparison.InvariantCultureIgnoreCase ) ? value.Substring( prefix.Length ) : value;

        /// <summary>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="rc"></param>
        /// <param name="ssp"></param>
        private static void ParseComposite( Uri uri, CompositeData rc, String ssp )
        {
            String componentString;
            String parms;

            if ( !CheckParenthesis( ssp ) )
                throw new NMSException( uri + ": Not a matching number of '(' and ')' parenthesis" );

            Int32 p;
            var intialParen = ssp.IndexOf( "(" );

            if ( intialParen >= 0 )
            {
                rc.Host = ssp.Substring( 0, intialParen );
                p = rc.Host.IndexOf( "/" );
                if ( p >= 0 )
                {
                    rc.Path = rc.Host.Substring( p );
                    rc.Host = rc.Host.Substring( 0, p );
                }

                p = ssp.LastIndexOf( ")" );
                var start = intialParen + 1;
                var len = p - start;
                componentString = ssp.Substring( start, len );
                parms = ssp.Substring( p + 1 )
                           .Trim();
            }
            else
            {
                componentString = ssp;
                parms = "";
            }

            var components = SplitComponents( componentString );
            rc.Components = new Uri[components.Length];
            for ( var i = 0; i < components.Length; i++ )
                rc.Components[i] = new Uri( components[i].Trim() );

            p = parms.IndexOf( "?" );
            if ( p >= 0 )
            {
                if ( p > 0 )
                    rc.Path = StripPrefix( parms.Substring( 0, p ), "/" );

                rc.Parameters = ParseQuery( parms.Substring( p + 1 ) );
            }
            else
            {
                if ( parms.Length > 0 )
                    rc.Path = StripPrefix( parms, "/" );

                rc.Parameters = EmptyMap;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="componentString"></param>
        private static String[] SplitComponents( String componentString )
        {
            var l = new ArrayList();

            var last = 0;
            var depth = 0;
            var chars = componentString.ToCharArray();
            for ( var i = 0; i < chars.Length; i++ )
                switch ( chars[i] )
                {
                    case '(':
                        depth++;
                        break;

                    case ')':
                        depth--;
                        break;

                    case ',':
                        if ( depth == 0 )
                        {
                            var s = componentString.Substring( last, i - last );
                            l.Add( s );
                            last = i + 1;
                        }
                        break;

                    default:
                        break;
                }

            var ending = componentString.Substring( last );
            if ( ending.Length != 0 )
                l.Add( ending );

            var rc = new String[l.Count];
            l.CopyTo( rc );
            return rc;
        }

        #region Nested Types

        public class CompositeData
        {
            #region Properties

            public Uri[] Components { get; set; }

            public String Fragment { get; set; }

            public Dictionary<String, String> Parameters { get; set; }

            public String Scheme { get; set; }

            public String Path { get; set; }

            public String Host { get; set; }

            #endregion

            public Uri toUri()
            {
                var sb = new StringBuilder();
                if ( Scheme != null )
                {
                    sb.Append( Scheme );
                    sb.Append( ':' );
                }

                if ( !String.IsNullOrEmpty( Host ) )
                {
                    sb.Append( Host );
                }
                else
                {
                    sb.Append( '(' );
                    for ( var i = 0; i < Components.Length; i++ )
                    {
                        if ( i != 0 )
                            sb.Append( ',' );
                        sb.Append( Components[i] );
                    }
                    sb.Append( ')' );
                }

                if ( Path != null )
                {
                    sb.Append( '/' );
                    sb.Append( Path );
                }

                if ( Parameters.Count != 0 )
                {
                    sb.Append( "?" );
                    sb.Append( CreateQueryString( Parameters ) );
                }

                if ( Fragment != null )
                {
                    sb.Append( "#" );
                    sb.Append( Fragment );
                }

                return new Uri( sb.ToString() );
            }
        }

        #endregion

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