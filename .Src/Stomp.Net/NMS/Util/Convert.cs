

#region Usings

using System;

#endregion

namespace Apache.NMS.Util
{
    public class NMSConvert
    {
        /// <summary>
        ///     Convert a text message into an object.  The object must be serializable from XML.
        /// </summary>
#if NET_3_5 || MONO
		[Obsolete]
#endif
        public static Object FromXmlMessage( IMessage message )
        {
            return DeserializeObjFromMessage( message );
        }

        /// <summary>
        ///     Convert the acknowledgment mode string into AcknowledgementMode enum.
        /// </summary>
        /// <param name="ackText"></param>
        /// <returns>Equivalent enum value.  If unknown string is encounted, it will default to AutoAcknowledge.</returns>
        public static AcknowledgementMode ToAcknowledgementMode( String ackText )
        {
            if ( String.Compare( ackText, "AutoAcknowledge", true ) == 0 )
                return AcknowledgementMode.AutoAcknowledge;
            if ( String.Compare( ackText, "ClientAcknowledge", true ) == 0 )
                return AcknowledgementMode.ClientAcknowledge;
            if ( String.Compare( ackText, "IndividualAcknowledge", true ) == 0 )
                return AcknowledgementMode.IndividualAcknowledge;
            if ( String.Compare( ackText, "DupsOkAcknowledge", true ) == 0 )
                return AcknowledgementMode.DupsOkAcknowledge;
            if ( String.Compare( ackText, "Transactional", true ) == 0 )
                return AcknowledgementMode.Transactional;
            return AcknowledgementMode.AutoAcknowledge;
        }

        /// <summary>
        ///     Convert an object into a text message.  The object must be serializable to XML.
        /// </summary>
#if NET_3_5 || MONO
		[Obsolete]
#endif
        public static ITextMessage ToXmlMessage( IMessageProducer producer, Object obj )
        {
            return SerializeObjToMessage( producer.CreateTextMessage(), obj );
        }

        /// <summary>
        ///     Convert an object into a text message.  The object must be serializable to XML.
        /// </summary>
#if NET_3_5 || MONO
		[Obsolete]
#endif
        public static ITextMessage ToXmlMessage( ISession session, Object obj )
        {
            return SerializeObjToMessage( session.CreateTextMessage(), obj );
        }

        /// <summary>
        ///     Deserialize the object from the text message.  The object must be serializable from XML.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static Object DeserializeObjFromMessage( IMessage message )
        {
            var textMessage = message as ITextMessage;

            if ( null == textMessage )
                return null;

            if ( String.IsNullOrEmpty( textMessage.NMSType ) )
            {
                Tracer.ErrorFormat( "NMSType not set on message.  Could not deserializing XML object." );
                return null;
            }

            var objType = GetRuntimeType( textMessage.NMSType );
            if ( null == objType )
            {
                Tracer.ErrorFormat( "Could not load type for {0} while deserializing XML object.", textMessage.NMSType );
                return null;
            }

            return XmlUtil.Deserialize( objType, textMessage.Text );
        }

        /// <summary>
        ///     Serialize the object as XML into the Text body of the message.
        ///     Set the NMSType to the full name of the object type.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static ITextMessage SerializeObjToMessage( ITextMessage message, Object obj )
        {
            // Embed the type into the message
            message.NMSType = obj.GetType()
                                 .FullName;
            message.Text = XmlUtil.Serialize( obj );
            return message;
        }

        /// <summary>
        ///     Get the runtime type for the class name.  This routine will search all loaded
        ///     assemblies in the current App Domain to find the type.
        /// </summary>
        /// <param name="typeName">Full name of the type.</param>
        /// <returns>Type object if found, or null if not found.</returns>
        private static Type GetRuntimeType( String typeName )
        {
            Type objType = null;

#if NETCF
			objType = Assembly.GetCallingAssembly().GetType(typeName, false);
#else
            foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                objType = assembly.GetType( typeName, false, true );
                if ( null != objType )
                    break;
            }
#endif

            return objType;
        }
    }
}