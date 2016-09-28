#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A life-cycle for STOM.Net objects to indicate they can be stopped.
    ///     Interface representing an object with start and stop functionality.
    /// </summary>
    public interface IStartStoppable
    {
        #region Properties

        /// <summary>
        ///     Gets a value indicating whether the object is started or not.
        /// </summary>
        /// <value>A value indicating whether the object is started or not.</value>
        Boolean IsStarted { get; }

        #endregion

        /// <summary>
        ///     Starts the object, if not yet started.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stops the object.
        /// </summary>
        void Stop();
    }
}