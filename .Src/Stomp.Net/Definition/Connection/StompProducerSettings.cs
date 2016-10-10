#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Class representing the STOMP producer settings.
    /// </summary>
    public class StompProducerSettings
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the dispatch async option.
        /// </summary>
        /// <value>The dispatch async option.</value>
        public Boolean DispatchAsync { get; set; }

        #endregion
    }
}