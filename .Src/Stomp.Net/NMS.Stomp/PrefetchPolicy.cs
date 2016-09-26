

#region Usings

using System;

#endregion

namespace Apache.NMS.Stomp
{
    /// <summary>
    ///     Class used to define the various limits that should be used for the Prefetch
    ///     limit on destination based on the type of Destination in use.
    /// </summary>
    public class PrefetchPolicy : ICloneable
    {
        #region Constants

        public const Int32 DEFAULT_DURABLE_TOPIC_PREFETCH = 100;
        public const Int32 DEFAULT_QUEUE_BROWSER_PREFETCH = 500;
        public const Int32 DEFAULT_QUEUE_PREFETCH = 1000;
        public const Int32 DEFAULT_TOPIC_PREFETCH = MAX_PREFETCH_SIZE;
        public const Int32 MAX_PREFETCH_SIZE = Int16.MaxValue - 1;

        #endregion

        #region Fields

        private Int32 durableTopicPrefetch;
        private Int32 queueBrowserPrefetch;

        private Int32 queuePrefetch;
        private Int32 topicPrefetch;

        #endregion

        #region Properties

        public Int32 QueuePrefetch
        {
            get { return queuePrefetch; }
            set { queuePrefetch = RestrictToMaximum( value ); }
        }

        public Int32 QueueBrowserPrefetch
        {
            get { return queueBrowserPrefetch; }
            set { queueBrowserPrefetch = RestrictToMaximum( value ); }
        }

        public Int32 TopicPrefetch
        {
            get { return topicPrefetch; }
            set { topicPrefetch = RestrictToMaximum( value ); }
        }

        public Int32 DurableTopicPrefetch
        {
            get { return durableTopicPrefetch; }
            set { durableTopicPrefetch = RestrictToMaximum( value ); }
        }

        public Int32 MaximumPendingMessageLimit { get; set; }

        #endregion

        #region Ctor

        public PrefetchPolicy()
        {
            queuePrefetch = DEFAULT_QUEUE_PREFETCH;
            queueBrowserPrefetch = DEFAULT_QUEUE_BROWSER_PREFETCH;
            topicPrefetch = DEFAULT_TOPIC_PREFETCH;
            durableTopicPrefetch = DEFAULT_DURABLE_TOPIC_PREFETCH;
        }

        #endregion

        public Object Clone() => MemberwiseClone();

        public void SetAll( Int32 value )
        {
            queuePrefetch = value;
            queueBrowserPrefetch = value;
            topicPrefetch = value;
            durableTopicPrefetch = value;
        }

        private static Int32 RestrictToMaximum( Int32 value ) => Math.Min( value, MAX_PREFETCH_SIZE );
    }
}