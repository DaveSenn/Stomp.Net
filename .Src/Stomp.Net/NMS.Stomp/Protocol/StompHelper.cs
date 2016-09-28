#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Protocol
{
    /// <summary>
    ///     Some <a href="http://stomp.codehaus.org/">STOMP</a> protocol conversion helper methods.
    /// </summary>
    public static class StompHelper
    {
        public static Boolean ToBool( String text, Boolean defaultValue )
        {
            if ( text == null )
                return defaultValue;

            return 0 == String.Compare( "true", text, StringComparison.OrdinalIgnoreCase );
        }

        public static String ToStomp( AcknowledgementMode ackMode )
            => ackMode == AcknowledgementMode.IndividualAcknowledge ? "client-individual" : "client";
    }
}