

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

        private PrimitiveMap body;
        private PrimitiveMapInterceptor typeConverter;

        #endregion

        #region Properties

        public override Boolean ReadOnlyBody
        {
            get { return base.ReadOnlyBody; }

            set
            {
                if ( typeConverter != null )
                    typeConverter.ReadOnly = true;

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
            this.body = body;
            typeConverter = new PrimitiveMapInterceptor( this, this.body );
        }

        #endregion

        public IPrimitiveMap Body
        {
            get
            {
                if ( body == null )
                {
                    body = new PrimitiveMap();
                    typeConverter = new PrimitiveMapInterceptor( this, body );
                }

                return typeConverter;
            }

            set
            {
                body = value as PrimitiveMap;
                typeConverter = value != null ? new PrimitiveMapInterceptor( this, value ) : null;
            }
        }

        public override void ClearBody()
        {
            body = null;
            typeConverter = null;
            base.ClearBody();
        }

        public override void BeforeMarshall( StompWireFormat wireFormat )
        {
            base.BeforeMarshall( wireFormat );

            Content = body == null ? null : wireFormat.MapMarshaler.Marshal( body );
        }

        public override Byte GetDataStructureType() => DataStructureTypes.MapMessageType;
    }
}