#region Usings

using System;

#endregion

namespace Stomp.Net.Stomp.Commands
{
    public class MessageId : BaseDataStructure
    {
        #region Fields

        private String _key;
        private Int64 _producerSequenceId;

        #endregion

        #region Properties

        public ProducerId ProducerId { get; set; }

        public Int64 ProducerSequenceId
        {
            get => _producerSequenceId;
            set => _producerSequenceId = value;
        }

        public Int64 BrokerSequenceId { get; set; }

        #endregion

        #region Ctor

        public MessageId()
        {
        }

        public MessageId( ProducerId prodId, Int64 producerSeqId )
        {
            ProducerId = prodId;
            _producerSequenceId = producerSeqId;
        }

        public MessageId( String value )
        {
            SetValue( value );
        }

        #endregion

        public override Boolean Equals( Object that )
        {
            if ( that is MessageId )
                return Equals( (MessageId) that );

            return false;
        }

        public virtual Boolean Equals( MessageId that ) => Equals( ProducerId, that.ProducerId )
                                                           && Equals( ProducerSequenceId, that.ProducerSequenceId )
                                                           && Equals( BrokerSequenceId, that.BrokerSequenceId );

        /// <summery>
        ///     Get the unique identifier that this object and its own
        ///     Marshaler share.
        /// </summery>
        public override Byte GetDataStructureType() => DataStructureTypes.MessageIdType;

        public override Int32 GetHashCode()
        {
            var answer = 0;

            answer = answer * 37 + HashCode( ProducerId );
            answer = answer * 37 + HashCode( ProducerSequenceId );
            answer = answer * 37 + HashCode( BrokerSequenceId );

            return answer;
        }

        /// <summery>
        ///     Returns a string containing the information for this DataStructure
        ///     such as its type and value of its elements.
        /// </summery>
        public override String ToString()
            => _key ?? ( _key = $"{ProducerId}:{_producerSequenceId}" );

        /// <summary>
        ///     Sets the value as a String
        /// </summary>
        private void SetValue( String messageKey )
        {
            var mkey = messageKey;

            _key = mkey;

            // Parse off the sequenceId
            var p = mkey.LastIndexOf( ":", StringComparison.Ordinal );
            if ( p >= 0 )
                if ( Int64.TryParse( mkey.Substring( p + 1 ), out _producerSequenceId ) )
                    mkey = mkey.Substring( 0, p );
                else
                    _producerSequenceId = 0;

            ProducerId = new ProducerId( mkey );
        }
    }
}