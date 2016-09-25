

using System;

namespace Apache.NMS.Stomp.Protocol
{
    /// <summary>
    ///     Some <a href="http://stomp.codehaus.org/">STOMP</a> protocol conversion helper methods.
    /// </summary>
    public class StompHelper
    {
        public static Boolean ToBool( String text, Boolean defaultValue )
        {
            if ( text == null )
                return defaultValue;

            return 0 == String.Compare( "true", text, true );
        }

        public static String ToStomp( AcknowledgementMode ackMode )
        {
            if ( ackMode == AcknowledgementMode.IndividualAcknowledge )
                return "client-individual";
            return "client";
        }
    }
}