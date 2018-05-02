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
        ///     Indicates if the Destination was created by this client or was provided
        ///     by the broker, most commonly the Destinations provided by the broker
        ///     are those that appear in the ReplyTo field of a Message.
        /// </summary>
        private Boolean RemoteDestination { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Destination" /> class with the given physical name.
        /// </summary>
        /// <param name="name">The physical name of the destination.</param>
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        protected Destination( String name, Boolean skipDesinationNameFormatting )
        {
            PhysicalName = name;
            SkipDesinationNameFormatting = skipDesinationNameFormatting;
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

        public String PhysicalName { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the destination name formatting should be skipped or not.
        ///     If set to true the physical name property will be used as stomp destination string without adding prefixes such as
        ///     queue or topic. This to support JMS brokers listening for queue/topic names in a different format.
        /// </summary>
        public Boolean SkipDesinationNameFormatting { get; }

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
        /// <param name="text">The name of the destination</param>
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        /// <returns></returns>
        public static Destination ConvertToDestination( String text, Boolean skipDesinationNameFormatting )
        {
            if ( text == null )
                return null;

            var type = StompQueue;
            var lowertext = text.ToLower();
            var remote = false;

            if ( lowertext.StartsWith( "/queue/", StringComparison.Ordinal ) )
                text = text.Substring( "/queue/".Length );
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

            return CreateDestination( type, text, remote, skipDesinationNameFormatting );
        }

        public String ConvertToStompString()
        {
            if ( SkipDesinationNameFormatting )
                return PhysicalName;

            String result;

            switch ( DestinationType )
            {
                case DestinationType.Topic:
                    result = "/topic/" + PhysicalName;
                    break;
                case DestinationType.TemporaryTopic:
                    result = ( RemoteDestination ? "/remote-temp-topic/" : "/temp-topic/" ) + PhysicalName;
                    break;
                case DestinationType.TemporaryQueue:
                    result = ( RemoteDestination ? "/remote-temp-queue/" : "/temp-queue/" ) + PhysicalName;
                    break;
                default:
                    result = "/queue/" + PhysicalName;
                    break;
            }

            return result;
        }

        /// <summary>
        ///     If the object passed in is equivalent, return true
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

        public static Destination Transform( IDestination destination )
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if ( destination == null )
                return null;

            if ( destination is Destination dest )
                return dest;

            if ( destination is ITemporaryQueue tempQueue )
                return new TempQueue( tempQueue.QueueName, tempQueue.SkipDesinationNameFormatting );

            if ( destination is ITemporaryTopic tempTopic )
                return new TempTopic( tempTopic.TopicName, tempTopic.SkipDesinationNameFormatting );

            if ( destination is IQueue queue )
                return new Queue( queue.QueueName, queue.SkipDesinationNameFormatting );

            return destination is ITopic topic
                ? new Topic( topic.TopicName, topic.SkipDesinationNameFormatting )
                : null;
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
        /// <param name="skipDesinationNameFormatting">
        ///     A value indicating whether the destination name formatting will be skipped
        ///     or not.
        /// </param>
        /// <returns></returns>
        private static Destination CreateDestination( Int32 type, String pyhsicalName, Boolean remote, Boolean skipDesinationNameFormatting )
        {
            Destination result;
            if ( pyhsicalName == null )
                return null;

            switch ( type )
            {
                case StompTopic:
                    result = new Topic( pyhsicalName, skipDesinationNameFormatting );
                    break;
                case StompTemporaryTopic:
                    result = new TempTopic( pyhsicalName, skipDesinationNameFormatting );
                    break;
                case StompQueue:
                    result = new Queue( pyhsicalName, skipDesinationNameFormatting );
                    break;
                default:
                    result = new TempQueue( pyhsicalName, skipDesinationNameFormatting );
                    break;
            }

            result.RemoteDestination = remote;

            return result;
        }
    }
}