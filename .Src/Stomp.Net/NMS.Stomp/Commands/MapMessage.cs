#region Usings

using System;
using Apache.NMS.Stomp.Protocol;
using Apache.NMS.Util;

#endregion

namespace Apache.NMS.Stomp.Commands
{
    public class MapMessage : Message, IMapMessage
    {
        #region Fields

        private PrimitiveMap _body;
        private PrimitiveMapInterceptor _typeConverter;

        #endregion

        #region Properties

        public override Boolean ReadOnlyBody
        {
            get { return base.ReadOnlyBody; }
            set
            {
                if ( _typeConverter != null )
                    _typeConverter.ReadOnly = true;

                base.ReadOnlyBody = value;
            }
        }

        #endregion

        #region Ctor

        public MapMessage()
        {
        }

        public MapMessage( PrimitiveMap body )
        {
            _body = body;
            _typeConverter = new PrimitiveMapInterceptor( this, _body );
        }

        #endregion

        public IPrimitiveMap Body
        {
            get
            {
                if ( _body == null )
                {
                    _body = new PrimitiveMap();
                    _typeConverter = new PrimitiveMapInterceptor( this, _body );
                }

                return _typeConverter;
            }

            set
            {
                _body = value as PrimitiveMap;
                _typeConverter = value != null ? new PrimitiveMapInterceptor( this, value ) : null;
            }
        }

        public override void ClearBody()
        {
            _body = null;
            _typeConverter = null;
            base.ClearBody();
        }

        public override void BeforeMarshall( StompWireFormat wireFormat )
        {
            base.BeforeMarshall( wireFormat );

            Content = _body == null ? null : wireFormat.MapMarshaler.Marshal( _body );
        }

        public override Byte GetDataStructureType()
            => DataStructureTypes.MapMessageType;
    }
}