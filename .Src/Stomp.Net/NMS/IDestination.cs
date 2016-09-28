#region Usings

using System;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     A base interface for destinations such as queues or topics
    /// </summary>
    public interface IDestination : IDisposable
    {
        #region Properties

        DestinationType DestinationType { get; }

        Boolean IsTopic { get; }
        Boolean IsQueue { get; }
        Boolean IsTemporary { get; }

        #endregion
    }
}