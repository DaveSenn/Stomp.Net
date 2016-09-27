#region Usings

using System;

#endregion

namespace Apache.NMS.Policies
{
    /// <summary>
    ///     A policy used to customize exactly how you want the redelivery to work.
    /// </summary>
    public class RedeliveryPolicy : IRedeliveryPolicy
    {
        #region Constants

        private static Boolean _nextBool;
        private static Random _randomNumberGenerator;
        private static readonly Object SyncObject = new Object();

        #endregion

        #region Fields

        private Double _collisionAvoidanceFactor = .15;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the random number generator.
        /// </summary>
        /// <value>The random number generator.</value>
        private static Random RandomNumberGenerator
        {
            get
            {
                if ( _randomNumberGenerator != null )
                    return _randomNumberGenerator;

                lock ( SyncObject )
                {
                    if ( _randomNumberGenerator != null )
                        return _randomNumberGenerator;

                    // ReSharper disable once PossibleMultipleWriteAccessInDoubleCheckLocking
                    return _randomNumberGenerator = new Random( DateTime.Now.Second );
                }
            }
        }

        /// <summary>
        ///     Gets the next boolean
        /// </summary>
        /// <value><c>true</c> if [next bool]; otherwise, <c>false</c>.</value>
        private static Boolean NextBool
        {
            get
            {
                lock ( SyncObject )
                {
                    _nextBool = !_nextBool;
                    return _nextBool;
                }
            }
        }

        #endregion

        /// <summery>
        ///     Clone this object and return a new instance that the caller now owns.
        /// </summery>
        public Object Clone()
            => MemberwiseClone();

        #region IRedeliveryPolicy Members

        public Int32 CollisionAvoidancePercent
        {
            get { return Convert.ToInt32( Math.Round( _collisionAvoidanceFactor * 100 ) ); }
            set { _collisionAvoidanceFactor = Convert.ToDouble( value ) * .01; }
        }

        public Boolean UseCollisionAvoidance { get; set; } = false;

        public Int32 InitialRedeliveryDelay { get; set; } = 1000;

        public Int32 MaximumRedeliveries { get; set; } = 6;

        public virtual Int32 RedeliveryDelay( Int32 redeliveredCounter )
        {
            Int32 delay;

            if ( redeliveredCounter == 0 )
                return 0;

            if ( UseExponentialBackOff && BackOffMultiplier > 1 )
                delay = InitialRedeliveryDelay * Convert.ToInt32( Math.Pow( BackOffMultiplier, redeliveredCounter - 1 ) );
            else
                delay = InitialRedeliveryDelay;

            if ( !UseCollisionAvoidance )
                return delay;
            var random = RandomNumberGenerator;
            var variance = ( NextBool ? _collisionAvoidanceFactor : _collisionAvoidanceFactor *= -1 ) * random.NextDouble();
            delay += Convert.ToInt32( Convert.ToDouble( delay ) * variance );

            return delay;
        }

        public Boolean UseExponentialBackOff { get; set; } = false;

        public Int32 BackOffMultiplier { get; set; } = 5;

        #endregion
    }
}