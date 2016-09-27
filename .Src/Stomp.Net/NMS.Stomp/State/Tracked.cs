#region Usings

using System;
using Apache.NMS.Stomp.Commands;

#endregion

namespace Apache.NMS.Stomp.State
{
    public class Tracked : Response
    {
        #region Properties

        public virtual Boolean WaitingForResponse => false;

        #endregion
        
        public void OnResponses()
        {
        }
    }
}