namespace Apache.NMS
{
    /// <summary>
    ///     Define an enumerated array of message delivery modes.  Provider-specific
    ///     values can be used to extend this enumerated mode.  TIBCO is known to
    ///     provide a third value of ReliableDelivery.  At minimum, a provider must
    ///     support Persistent and NonPersistent.
    /// </summary>
    public enum MessageDeliveryMode
    {
        Persistent,
        NonPersistent
    }
}