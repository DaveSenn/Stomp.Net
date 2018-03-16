#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace Stomp.Net.Stomp
{
    /// <summary>
    ///     Class used to define the various limits that should be used for the Prefetch
    ///     limit on destination based on the type of Destination in use.
    /// </summary>
    public class PrefetchPolicy : ICloneable
    {
        #region Constants

        private const Int32 DefaultDurableTopicPrefetch = 100;
        private const Int32 DefaultQueuePrefetch = 1000;
        private const Int32 DefaultTopicPrefetch = MaxPrefetchSize;
        private const Int32 MaxPrefetchSize = Int16.MaxValue - 1;

        #endregion

        #region Fields

        private Int32 _durableTopicPrefetch;
        private Int32 _queuePrefetch;
        private Int32 _topicPrefetch;

        #endregion

        #region Properties

        [PublicAPI]
        public Int32 QueuePrefetch
        {
            get => _queuePrefetch;
            set => _queuePrefetch = RestrictToMaximum( value );
        }

        [PublicAPI]
        public Int32 TopicPrefetch
        {
            get => _topicPrefetch;
            set => _topicPrefetch = RestrictToMaximum( value );
        }

        [PublicAPI]
        public Int32 DurableTopicPrefetch
        {
            get => _durableTopicPrefetch;
            set => _durableTopicPrefetch = RestrictToMaximum( value );
        }

        public Int32 MaximumPendingMessageLimit { get; set; }

        #endregion

        #region Ctor

        public PrefetchPolicy()
        {
            _queuePrefetch = DefaultQueuePrefetch;
            _topicPrefetch = DefaultTopicPrefetch;
            _durableTopicPrefetch = DefaultDurableTopicPrefetch;
        }

        #endregion

        public Object Clone()
            => MemberwiseClone();

        public void SetAll( Int32 value )
        {
            _queuePrefetch = value;
            _topicPrefetch = value;
            _durableTopicPrefetch = value;
        }

        private static Int32 RestrictToMaximum( Int32 value )
            => Math.Min( value, MaxPrefetchSize );
    }
}