namespace Stomp.Net.Stomp
{
    public enum AckType
    {
        ConsumedAck = 1, // Message consumed, discard
        IndividualAck = 2 // Only the given message is to be treated as consumed.
    }
}