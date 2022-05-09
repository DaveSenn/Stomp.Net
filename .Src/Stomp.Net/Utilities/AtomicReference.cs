namespace Stomp.Net.Utilities;

public class AtomicReference<T>
{
    #region Fields

    protected T AtomicValue;

    #endregion

    #region Properties

    public T Value
    {
        get
        {
            lock ( this )
                return AtomicValue;
        }
        set
        {
            lock ( this )
                AtomicValue = value;
        }
    }

    #endregion

    #region Ctor

    public AtomicReference()
        => AtomicValue = default;

    public AtomicReference( T defaultValue )
        => AtomicValue = defaultValue;

    #endregion
}