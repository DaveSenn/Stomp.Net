#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    /// <summary>
    ///     Summary description for Destination.
    /// </summary>
    public abstract class Destination : BaseDataStructure, IDestination
    {
        #region Constants

        /// <summary>
        ///     Queue Destination object
        /// </summary>
        protected const Int32 StompQueue = 3;

        /// <summary>
        ///     Temporary Queue Destination object
        /// </summary>
        protected const Int32 StompTemporaryQueue = 4;

        /// <summary>
        ///     Temporary Topic Destination object
        /// </summary>
        protected const Int32 StompTemporaryTopic = 2;

        /// <summary>
        ///     Topic Destination object
        /// </summary>
        protected const Int32 StompTopic = 1;

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates if the Desination was created by this client or was provided
        ///     by the broker, most commonly the deinstinations provided by the broker
        ///     are those that appear in the ReplyTo field of a Message.
        /// </summary>
        private Boolean RemoteDestination { get; set; }

        public String PhysicalName { get; } = String.Empty;

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
            PhysicalName = name;
        }

        #endregion

        public abstract DestinationType DestinationType { get; }

        public Boolean IsQueue
        {
            get
            {
                var destinationType = GetDestinationType();
                return StompQueue == destinationType
                       || StompTemporaryQueue == destinationType;
            }
        }

        public Boolean IsTemporary
        {
            get
            {
                var destinationType = GetDestinationType();
                return StompTemporaryQueue == destinationType
                       || StompTemporaryTopic == destinationType;
            }
        }

        public Boolean IsTopic
        {
            get
            {
                var destinationType = GetDestinationType();
                return StompTopic == destinationType
                       || StompTemporaryTopic == destinationType;
            }
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

        public static Destination ConvertToDestination( String text )
        {
            if ( text == null )
                return null;

            var type = StompQueue;
            var lowertext = text.ToLower();
            var remote = false;

            if ( lowertext.StartsWith( "/queue/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/queue/".Length );
            }
            else if ( lowertext.StartsWith( "/topic/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/topic/".Length );
                type = StompTopic;
            }
            else if ( lowertext.StartsWith( "/temp-topic/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/temp-topic/".Length );
                type = StompTemporaryTopic;
            }
            else if ( lowertext.StartsWith( "/temp-queue/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/temp-queue/".Length );
                type = StompTemporaryQueue;
            }
            else if ( lowertext.StartsWith( "/remote-temp-topic/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/remote-temp-topic/".Length );
                type = StompTemporaryTopic;
                remote = true;
            }
            else if ( lowertext.StartsWith( "/remote-temp-queue/", StringComparison.Ordinal ) )
            {
                text = text.Substring( "/remote-temp-queue/".Length );
                type = StompTemporaryQueue;
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
            if ( result || !( obj is Destination ) )
                return result;
            var other = (Destination) obj;
            result = GetDestinationType() == other.GetDestinationType()
                     && PhysicalName.Equals( other.PhysicalName );
            return result;
        }

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
            if ( destination == null )
                return null;

            if ( destination is Destination )
                result = (Destination) destination;
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

        /// <summary>
        /// </summary>
        /// <returns>Returns the Destination type</returns>
        protected abstract Int32 GetDestinationType();

        /// <summary>
        ///     Create a Destination using the name given, the type is determined by the
        ///     value of the type parameter.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pyhsicalName"></param>
        /// <param name="remote"></param>
        /// <returns></returns>
        private static Destination CreateDestination( Int32 type, String pyhsicalName, Boolean remote )
        {
            Destination result;
            if ( pyhsicalName == null )
                return null;
            switch ( type )
            {
                case StompTopic:
                    result = new Topic( pyhsicalName );
                    break;
                case StompTemporaryTopic:
                    result = new TempTopic( pyhsicalName );
                    break;
                case StompQueue:
                    result = new Queue( pyhsicalName );
                    break;
                default:
                    result = new TempQueue( pyhsicalName );
                    break;
            }

            result.RemoteDestination = remote;

            return result;
        }
    }
}