#region Usings

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

#endregion

namespace Apache.NMS.Util
{
    /// <summary>
    ///     Class to provide support for working with Xml objects.
    /// </summary>
    public class XmlUtil
    {
        #region Constants

        /// <summary>
        ///     From xml spec valid chars:
        ///     #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
        ///     any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
        /// </summary>
        private const String invalidXMLMatch = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";

        private static readonly XmlWriterSettings xmlWriterSettings;
        private static readonly Regex regexInvalidXMLChars = new Regex( invalidXMLMatch );

        #endregion

        #region Ctor

        /// <summary>
        ///     Static class constructor.
        /// </summary>
        static XmlUtil()
        {
            xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Encoding = new UTF8Encoding( false, false );
        }

        #endregion

        /// <summary>
        ///     This removes characters that are invalid for xml encoding
        /// </summary>
        /// <param name="text">Text to be encoded.</param>
        /// <returns>Text with invalid xml characters removed.</returns>
        public static String CleanInvalidXmlChars( String text ) => regexInvalidXMLChars.Replace( text, "" );

        public static Object Deserialize( Type objType, String text )
        {
            if ( null == text )
                return null;

            try
            {
                var serializer = new XmlSerializer( objType );

                // Set the error handlers.
                serializer.UnknownNode += serializer_UnknownNode;
                serializer.UnknownElement += serializer_UnknownElement;
                serializer.UnknownAttribute += serializer_UnknownAttribute;
                return serializer.Deserialize( new StringReader( text ) );
            }
            catch ( Exception ex )
            {
                Tracer.ErrorFormat( "Error deserializing object: {0}", ex.Message );
                return null;
            }
        }

        /// <summary>
        ///     Serialize the object to XML format.  The XML encoding will be UTF-8.  A Byte Order Mark (BOM)
        ///     will NOT be placed at the beginning of the string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String Serialize( Object obj )
        {
            try
            {
                Byte[] encodedBytes;

                using ( var outputStream = new MemoryStream() )
                    using ( var xmlWriter = XmlWriter.Create( outputStream, xmlWriterSettings ) )
                    {
                        var serializer = new XmlSerializer( obj.GetType() );

                        // Set the error handlers.
                        serializer.UnknownNode += serializer_UnknownNode;
                        serializer.UnknownElement += serializer_UnknownElement;
                        serializer.UnknownAttribute += serializer_UnknownAttribute;
                        serializer.Serialize( xmlWriter, obj );
                        encodedBytes = outputStream.ToArray();
                    }

                return xmlWriterSettings.Encoding.GetString( encodedBytes, 0, encodedBytes.Length );
            }
            catch ( Exception ex )
            {
                Tracer.ErrorFormat( "Error serializing object: {0}", ex.Message );
                return null;
            }
        }

        private static void serializer_UnknownAttribute( Object sender, XmlAttributeEventArgs e ) => Tracer.ErrorFormat( "Unknown attribute: {0}='{1}'", e.Attr.Name, e.Attr.Value );

        private static void serializer_UnknownElement( Object sender, XmlElementEventArgs e ) => Tracer.ErrorFormat( "Unknown Element: {0}\t{1}", e.Element.Name, e.Element.Value );

        private static void serializer_UnknownNode( Object sender, XmlNodeEventArgs e ) => Tracer.ErrorFormat( "Unknown Node: {0}\t{1}", e.Name, e.Text );
    }
}