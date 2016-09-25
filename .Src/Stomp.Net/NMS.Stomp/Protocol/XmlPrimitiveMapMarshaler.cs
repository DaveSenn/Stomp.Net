// /*
//  * Licensed to the Apache Software Foundation (ASF) under one or more
//  * contributor license agreements.  See the NOTICE file distributed with
//  * this work for additional information regarding copyright ownership.
//  * The ASF licenses this file to You under the Apache License, Version 2.0
//  * (the "License"); you may not use this file except in compliance with
//  * the License.  You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */
// 

#region Usings

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Protocol
{
    /// <summary>
    ///     Reads / Writes an IPrimitveMap as XML compatible with XStream.
    /// </summary>
    public class XmlPrimitiveMapMarshaler : IPrimitiveMapMarshaler
    {
        #region Fields

        private readonly Encoding encoder = new UTF8Encoding();

        #endregion

        #region Ctor

        public XmlPrimitiveMapMarshaler()
        {
        }

        public XmlPrimitiveMapMarshaler( Encoding encoder )
        {
            this.encoder = encoder;
        }

        #endregion

        public Byte[] Marshal( IPrimitiveMap map )
        {
            if ( map == null )
                return null;

            var builder = new StringBuilder();

            var settings = new XmlWriterSettings();

            settings.OmitXmlDeclaration = true;
            settings.Encoding = encoder;
            settings.NewLineHandling = NewLineHandling.None;

            var writer = XmlWriter.Create( builder, settings );

            writer.WriteStartElement( "map" );

            foreach ( String entry in map.Keys )
            {
                writer.WriteStartElement( "entry" );

                // Encode the Key <string>key</string>
                writer.WriteElementString( "string", entry );

                var value = map[entry];

                // Encode the Value <${type}>value</${type}>
                MarshalPrimitive( writer, value );

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();

            return encoder.GetBytes( builder.ToString() );
        }

        public String Name
        {
            get { return "jms-map-xml"; }
        }

        public IPrimitiveMap Unmarshal( Byte[] mapContent )
        {
            var xmlString = encoder.GetString( mapContent, 0, mapContent.Length );

            var result = new PrimitiveMap();

            if ( xmlString == null || xmlString == "" )
                return result;

            var settings = new XmlReaderSettings();

            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;

            var reader = XmlReader.Create( new StringReader( xmlString ), settings );

            reader.MoveToContent();
            reader.ReadStartElement( "map" );

            while ( reader.Name == "entry" )
            {
                reader.ReadStartElement();
                var key = reader.ReadElementContentAsString( "string", "" );

                Object value = null;

                switch ( reader.Name )
                {
                    case "char":
                        value = Convert.ToChar( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "double":
                        value = Convert.ToDouble( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "float":
                        value = Convert.ToSingle( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "long":
                        value = Convert.ToInt64( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "int":
                        value = Convert.ToInt32( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "short":
                        value = Convert.ToInt16( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "byte":
                        value = (Byte) Convert.ToInt16( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "boolean":
                        value = Convert.ToBoolean( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    case "byte-array":
                        value = Convert.FromBase64String( reader.ReadElementContentAsString() );
                        reader.ReadEndElement();
                        break;
                    default:
                        value = reader.ReadElementContentAsString();
                        reader.ReadEndElement();
                        break;
                }
                ;

                // Now store the value into our new PrimitiveMap.
                result[key] = value;
            }

            reader.Close();

            return result;
        }

        private void MarshalPrimitive( XmlWriter writer, Object value )
        {
            if ( value == null )
                throw new NullReferenceException( "PrimitiveMap values should not be Null" );
            if ( value is Char )
            {
                writer.WriteElementString( "char", value.ToString() );
            }
            else if ( value is Boolean )
            {
                writer.WriteElementString( "boolean",
                                           value.ToString()
                                                .ToLower() );
            }
            else if ( value is Byte )
            {
                writer.WriteElementString( "byte", ( (Byte) value ).ToString() );
            }
            else if ( value is Int16 )
            {
                writer.WriteElementString( "short", value.ToString() );
            }
            else if ( value is Int32 )
            {
                writer.WriteElementString( "int", value.ToString() );
            }
            else if ( value is Int64 )
            {
                writer.WriteElementString( "long", value.ToString() );
            }
            else if ( value is Single )
            {
                writer.WriteElementString( "float", value.ToString() );
            }
            else if ( value is Double )
            {
                writer.WriteElementString( "double", value.ToString() );
            }
            else if ( value is Byte[] )
            {
                writer.WriteElementString( "byte-array", Convert.ToBase64String( (Byte[]) value ) );
            }
            else if ( value is String )
            {
                writer.WriteElementString( "string", (String) value );
            }
            else if ( value is IDictionary )
            {
                Tracer.Debug( "Can't Marshal a Dictionary" );

                throw new NotSupportedException( "Can't marshal nested Maps in Stomp" );
            }
            else if ( value is IList )
            {
                Tracer.Debug( "Can't Marshal a List" );

                throw new NotSupportedException( "Can't marshal nested Maps in Stomp" );
            }
            else
            {
                Console.WriteLine( "Can't Marshal a something other than a Primitive Value." );

                throw new Exception( "Object is not a primitive: " + value );
            }
        }
    }
}