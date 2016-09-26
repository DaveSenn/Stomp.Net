

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Destination.
    /// </summary>
    public abstract class Destination : BaseDataStructure, IDestination
    {
        #region Constants

//        private const String TEMP_PREFIX = "{TD{";
//        private const String TEMP_POSTFIX = "}TD}";
        private const String COMPOSITE_SEPARATOR = ",";

        /// <summary>
        ///     Queue Destination object
        /// </summary>
        public const Int32 STOMP_QUEUE = 3;

        /// <summary>
        ///     Temporary Queue Destination object
        /// </summary>
        public const Int32 STOMP_TEMPORARY_QUEUE = 4;

        /// <summary>
        ///     Temporary Topic Destination object
        /// </summary>
        public const Int32 STOMP_TEMPORARY_TOPIC = 2;

        /// <summary>
        ///     Topic Destination object
        /// </summary>
        public const Int32 STOMP_TOPIC = 1;

        #endregion

        #region Fields

        #endregion

        #region Properties

        /// <summary>
        ///     Dictionary of name/value pairs representing option values specified
        ///     in the URI used to create this Destination.  A null value is returned
        ///     if no options were specified.
        /// </summary>
        internal Dictionary<String, String> Options { get; private set; }

        /// <summary>
        ///     Indicates if the Desination was created by this client or was provided
        ///     by the broker, most commonly the deinstinations provided by the broker
        ///     are those that appear in the ReplyTo field of a Message.
        /// </summary>
        internal Boolean RemoteDestination { get; set; }

        public String PhysicalName { get; set; } = "";

        /// <summary>
        ///     Returns true if this destination represents a collection of
        ///     destinations; allowing a set of destinations to be published to or subscribed
        ///     from in one NMS operation.
        /// </summary>
        public Boolean IsComposite
        {
            get { return PhysicalName.IndexOf( COMPOSITE_SEPARATOR ) > 0; }
        }

        #endregion

        #region Ctor

        /// <summary>
        ///     The Default Constructor
        /// </summary>
        protected Destination()
        {
        }

        /// <summary>
        ///     Construct the Destination with a defined physical name;
        /// </summary>
        /// <param name="name"></param>
        protected Destination( String name )
        {
            SetPhysicalName( name );
        }

        #endregion

        public abstract DestinationType DestinationType { get; }

        public Boolean IsQueue
        {
            get
            {
                var destinationType = GetDestinationType();
                return STOMP_QUEUE == destinationType
                       || STOMP_TEMPORARY_QUEUE == destinationType;
            }
        }

        public Boolean IsTemporary
        {
            get
            {
                var destinationType = GetDestinationType();
                return STOMP_TEMPORARY_QUEUE == destinationType
                       || STOMP_TEMPORARY_TOPIC == destinationType;
            }
        }

        public Boolean IsTopic
        {
            get
            {
                var destinationType = GetDestinationType();
                return STOMP_TOPIC == destinationType
                       || STOMP_TEMPORARY_TOPIC == destinationType;
            }
        }

        public void Dispose()
        {
        }

        public override Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            var o = (Destination) base.Clone();

            // Now do the deep work required
            // If any new variables are added then this routine will
            // likely need updating

            return o;
        }

        /// <summary>
        /// </summary>
        /// <param name="o">object to compare</param>
        /// <returns>1 if this is less than o else 0 if they are equal or -1 if this is less than o</returns>
        public Int32 CompareTo( Object o )
        {
            if ( o is Destination )
                return CompareTo( (Destination) o );
            return -1;
        }

        /// <summary>
        ///     Lets sort by name first then lets sort topics greater than queues
        /// </summary>
        /// <param name="that">another destination to compare against</param>
        /// <returns>1 if this is less than o else 0 if they are equal or -1 if this is less than o</returns>
        public Int32 CompareTo( Destination that )
        {
            var answer = 0;
            if ( PhysicalName != that.PhysicalName )
            {
                if ( PhysicalName == null )
                    return -1;
                if ( that.PhysicalName == null )
                    return 1;
                answer = PhysicalName.CompareTo( that.PhysicalName );
            }

            if ( answer == 0 )
                if ( IsTopic )
                {
                    if ( that.IsQueue )
                        return 1;
                }
                else
                {
                    if ( that.IsTopic )
                        return -1;
                }
            return answer;
        }

        public static Destination ConvertToDestination( String text )
        {
            if ( text == null )
                return null;

            var type = STOMP_QUEUE;
            var lowertext = text.ToLower();
            var remote = false;

            if ( lowertext.StartsWith( "/queue/" ) )
            {
                text = text.Substring( "/queue/".Length );
            }
            else if ( lowertext.StartsWith( "/topic/" ) )
            {
                text = text.Substring( "/topic/".Length );
                type = STOMP_TOPIC;
            }
            else if ( lowertext.StartsWith( "/temp-topic/" ) )
            {
                text = text.Substring( "/temp-topic/".Length );
                type = STOMP_TEMPORARY_TOPIC;
            }
            else if ( lowertext.StartsWith( "/temp-queue/" ) )
            {
                text = text.Substring( "/temp-queue/".Length );
                type = STOMP_TEMPORARY_QUEUE;
            }
            else if ( lowertext.StartsWith( "/remote-temp-topic/" ) )
            {
                text = text.Substring( "/remote-temp-topic/".Length );
                type = STOMP_TEMPORARY_TOPIC;
                remote = true;
            }
            else if ( lowertext.StartsWith( "/remote-temp-queue/" ) )
            {
                text = text.Substring( "/remote-temp-queue/".Length );
                type = STOMP_TEMPORARY_QUEUE;
                remote = true;
            }

            return CreateDestination( type, text, remote );
        }

        public static String ConvertToStompString( Destination destination )
        {
            if ( destination == null )
                return null;

            String result;

            switch ( destination.DestinationType )
            {
                case DestinationType.Topic:
                    result = "/topic/" + destination.PhysicalName;
                    break;
                case DestinationType.TemporaryTopic:
                    result = ( destination.RemoteDestination ? "/remote-temp-topic/" : "/temp-topic/" ) + destination.PhysicalName;
                    break;
                case DestinationType.TemporaryQueue:
                    result = ( destination.RemoteDestination ? "/remote-temp-queue/" : "/temp-queue/" ) + destination.PhysicalName;
                    break;
                default:
                    result = "/queue/" + destination.PhysicalName;
                    break;
            }

            return result;
        }

        /// <summary>
        ///     Create a Destination using the name given, the type is determined by the
        ///     value of the type parameter.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pyhsicalName"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        public static Destination CreateDestination( Int32 type, String pyhsicalName, Boolean remote )
        {
            Destination result = null;
            if ( pyhsicalName == null )
                return null;
            switch ( type )
            {
                case STOMP_TOPIC:
                    result = new Topic( pyhsicalName );
                    break;
                case STOMP_TEMPORARY_TOPIC:
                    result = new TempTopic( pyhsicalName );
                    break;
                case STOMP_QUEUE:
                    result = new Queue( pyhsicalName );
                    break;
                default:
                    result = new TempQueue( pyhsicalName );
                    break;
            }

            result.RemoteDestination = remote;

            return result;
        }

        /// <summary>
        ///     Factory method to create a child destination if this destination is a composite
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the created Destination</returns>
        public abstract Destination CreateDestination( String name );

        /// <summary>
        ///     if the object passed in is equivalent, return true
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if this instance and obj are equivalent</returns>
        public override Boolean Equals( Object obj )
        {
            var result = this == obj;
            if ( !result && obj != null && obj is Destination )
            {
                var other = (Destination) obj;
                result = GetDestinationType() == other.GetDestinationType()
                         && PhysicalName.Equals( other.PhysicalName );
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns>Returns the Destination type</returns>
        public abstract Int32 GetDestinationType();

        /// <summary>
        /// </summary>
        /// <returns>hashCode for this instance</returns>
        public override Int32 GetHashCode()
        {
            var answer = 37;

            if ( PhysicalName != null )
                answer = PhysicalName.GetHashCode();
            if ( IsTopic )
                answer ^= 0xfabfab;
            return answer;
        }

        /// <summary>
        /// </summary>
        /// <returns>string representation of this instance</returns>
        public override String ToString()
        {
            switch ( DestinationType )
            {
                case DestinationType.Topic:
                    return "topic://" + PhysicalName;

                case DestinationType.TemporaryTopic:
                    return "temp-topic://" + PhysicalName;

                case DestinationType.TemporaryQueue:
                    return "temp-queue://" + PhysicalName;

                default:
                    return "queue://" + PhysicalName;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static Destination Transform( IDestination destination )
        {
            Destination result = null;
            if ( destination != null )
                if ( destination is Destination )
                {
                    result = (Destination) destination;
                }
                else
                {
                    if ( destination is ITemporaryQueue )
                        result = new TempQueue( ( (IQueue) destination ).QueueName );
                    else if ( destination is ITemporaryTopic )
                        result = new TempTopic( ( (ITopic) destination ).TopicName );
                    else if ( destination is IQueue )
                        result = new Queue( ( (IQueue) destination ).QueueName );
                    else if ( destination is ITopic )
                        result = new Topic( ( (ITopic) destination ).TopicName );
                }
            return result;
        }

        private void SetPhysicalName( String name )
        {
            PhysicalName = name;

            var p = name.IndexOf( '?' );
            if ( p >= 0 )
            {
                var optstring = PhysicalName.Substring( p + 1 );
                PhysicalName = name.Substring( 0, p );
                Options = URISupport.ParseQuery( optstring );
            }
        }
    }
}