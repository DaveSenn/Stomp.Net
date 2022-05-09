#region Usings

using System;

#endregion

namespace Stomp.Net;

public interface ICloneable
{
    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns></returns>
    Object Clone();
}