namespace Apache.NMS
{
    /// <summary>
    ///     Represents a Map message which contains key and value pairs which are
    ///     of primitive types
    /// </summary>
    public interface IMapMessage : IMessage
    {
        #region Properties

        IPrimitiveMap Body { get; }

        #endregion
    }
}