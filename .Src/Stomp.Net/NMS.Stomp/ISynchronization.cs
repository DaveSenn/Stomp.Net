namespace Apache.NMS.Stomp
{
    public interface ISynchronization
    {
        /// <summary>
        ///     Called after a commit
        /// </summary>
        void AfterCommit();

        /// <summary>
        ///     Called after a transaction rollback
        /// </summary>
        void AfterRollback();

        /// <summary>
        ///     Called before a commit or rollback is applied.
        /// </summary>
        void BeforeEnd();
    }
}