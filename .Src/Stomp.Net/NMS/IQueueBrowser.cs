

#region Usings

using System;
using System.Collections;

#endregion

namespace Apache.NMS
{
    /// <summary>
    ///     A client uses a QueueBrowser object to look at messages on a queue without removing them.
    ///     The Enumeration method returns a java.util.Enumeration that is used to scan the queue's
    ///     messages. It may be an enumeration of the entire content of a queue, or it may contain
    ///     only the messages matching a message selector.
    ///     Messages may be arriving and expiring while the scan is done. The NMS API does not
    ///     require the content of an enumeration to be a static snapshot of queue content. Whether
    ///     these changes are visible or not depends on the NMS provider.
    /// </summary>
    public interface IQueueBrowser : IEnumerable, IDisposable
    {
        #region Properties

        /// <value>
        ///     Gets this queue browser's message selector expression.  If no Message
        ///     selector was specified than this method returns null.
        /// </value>
        /// <exception cref="Apache.NMS.NMSException">
        ///     If NMS Provider fails to get the Message Selector for some reason.
        /// </exception>
        String MessageSelector { get; }

        /// <value>
        ///     Gets the queue associated with this queue browser.
        /// </value>
        /// <exception cref="Apache.NMS.NMSException">
        ///     If NMS Provider fails to retrieve the IQueue associated with the Browser
        ///     doe to some internal error.
        /// </exception>
        IQueue Queue { get; }

        #endregion

        /// <summary>
        ///     Closes the QueueBrowser.
        /// </summary>
        /// <exception cref="Apache.NMS.NMSException">
        ///     If NMS Provider fails to close the Browser for some reason.
        /// </exception>
        void Close();
    }
}