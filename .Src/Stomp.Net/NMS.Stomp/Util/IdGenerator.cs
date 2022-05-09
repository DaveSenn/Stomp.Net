#region Usings

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#endregion

namespace Stomp.Net.Stomp.Util;

public class IdGenerator
{
    #region Properties

    /// <summary>
    ///     As we have to find the hostname as a side-affect of generating a unique
    ///     stub, we allow it's easy retrevial here
    /// </summary>
    public static String HostName { get; }

    #endregion

    /// <summary>
    ///     Generate a Unique Id
    /// </summary>
    public String GenerateId()
    {
        lock ( UniqueStub )
            return _seed + _sequence++;
    }

    #region Constants

    private static Int32 _instanceCount;
    private static readonly String UniqueStub;

    #endregion

    #region Fields

    private readonly String _seed;
    private Int64 _sequence;

    #endregion

    #region Ctor

    static IdGenerator()
    {
        var stub = "-1-" + DateTime.Now.Ticks;
        HostName = "localhost";

        try
        {
            HostName = Dns.GetHostName();
            var endPoint = new IPEndPoint( IPAddress.Any, 0 );
            using var tempSocket = new Socket( endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp );
            tempSocket.Bind( endPoint );
            stub = "-" + ( (IPEndPoint) tempSocket.LocalEndPoint ).Port + "-" + DateTime.Now.Ticks + "-";
            Thread.Sleep( 100 );
        }
        catch ( Exception ioe )
        {
            if ( Tracer.IsWarnEnabled )
                Tracer.Warn( "could not generate unique stub: " + ioe.Message );
        }

        UniqueStub = stub;
    }

    /**
         * Construct an IdGenerator
         */
    public IdGenerator( String prefix )
    {
        lock ( UniqueStub )
            _seed = prefix + UniqueStub + _instanceCount++ + ":";
    }

    public IdGenerator()
        : this( "ID:" + HostName )
    {
    }

    #endregion
}