

using System;

namespace Apache.NMS
{
    /// <summary>
    ///     A lifecycle for NMS objects to indicate they can be started
    /// </summary>
    public interface IStartable
    {
        #region Properties

        Boolean IsStarted { get; }

        #endregion

        void Start();
    }
}