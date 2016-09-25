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

        private static Boolean nextBool;

        private static Random randomNumberGenerator;
        private static readonly Object syncObject = new Object();

        #endregion

        #region Fields

        private Double collisionAvoidanceFactor = .15;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the random number generator.
        /// </summary>
        /// <value>The random number generator.</value>
        protected static Random RandomNumberGenerator
        {
            get
            {
                if ( randomNumberGenerator == null )
                    lock ( syncObject )
                        if ( randomNumberGenerator == null )
                            randomNumberGenerator = new Random( DateTime.Now.Second );

                return randomNumberGenerator;
            }
        }

        /// <summary>
        ///     Gets the next boolean
        /// </summary>
        /// <value><c>true</c> if [next bool]; otherwise, <c>false</c>.</value>
        protected static Boolean NextBool
        {
            get
            {
                lock ( syncObject )
                {
                    nextBool = !nextBool;
                    return nextBool;
                }
            }
        }

        #endregion

        /// <summery>
        ///     Clone this object and return a new instance that the caller now owns.
        /// </summery>
        public Object Clone()
        {
            // Since we are a derived class use the base's Clone()
            // to perform the shallow copy. Since it is shallow it
            // will include our derived class. Since we are derived,
            // this method is an override.
            return MemberwiseClone();
        }

        #region IRedeliveryPolicy Members

        public Int32 CollisionAvoidancePercent
        {
            get { return Convert.ToInt32( Math.Round( collisionAvoidanceFactor * 100 ) ); }
            set { collisionAvoidanceFactor = Convert.ToDouble( value ) * .01; }
        }

        public Boolean UseCollisionAvoidance { get; set; } = false;

        public Int32 InitialRedeliveryDelay { get; set; } = 1000;

        public Int32 MaximumRedeliveries { get; set; } = 6;

        public virtual Int32 RedeliveryDelay( Int32 redeliveredCounter )
        {
            var delay = 0;

            if ( redeliveredCounter == 0 )
                return 0;

            if ( UseExponentialBackOff && BackOffMultiplier > 1 )
                delay = InitialRedeliveryDelay * Convert.ToInt32( Math.Pow( BackOffMultiplier, redeliveredCounter - 1 ) );
            else
                delay = InitialRedeliveryDelay;

            if ( UseCollisionAvoidance )
            {
                var random = RandomNumberGenerator;
                var variance = ( NextBool ? collisionAvoidanceFactor : collisionAvoidanceFactor *= -1 ) * random.NextDouble();
                delay += Convert.ToInt32( Convert.ToDouble( delay ) * variance );
            }

            return delay;
        }

        public Boolean UseExponentialBackOff { get; set; } = false;

        public Int32 BackOffMultiplier { get; set; } = 5;

        #endregion
    }
}