#region Usings

using System;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class ConsumerState
    {
        #region Properties

        public ConsumerInfo Info { get; }

        #endregion

        #region Ctor

        public ConsumerState( ConsumerInfo info )
        {
            Info = info;
        }

        #endregion

        public override String ToString() => Info.ToString();
    }
}