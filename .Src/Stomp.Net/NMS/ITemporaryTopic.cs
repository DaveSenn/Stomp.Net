

namespace Apache.NMS
{
    /// <summary>
    ///     Represents a temporary topic which exists for the duration
    ///     of the IConnection which created it.
    /// </summary>
    public interface ITemporaryTopic : ITopic
    {
        /// <summary>
        ///     Deletes this Temporary Destination, If there are existing receivers
        ///     still using it, a NMSException will be thrown.
        /// </summary>
        /// <exception cref="Apache.NMS.NMSException">
        ///     If NMS Provider fails to Delete the Temp Destination or the client does
        ///     not support this operation.
        /// </exception>
        void Delete();
    }
}