#region Usings

using System;
using System.Diagnostics;

#endregion

namespace Stomp.Net
{
    /// <summary>
    ///     Base class for disposable Objects.
    /// </summary>
    [DebuggerStepThrough]
    public abstract class Disposable : IDisposable
    {
        #region Fields

        /// <summary>
        ///     Stores whether the instance is disposed or not.
        /// </summary>
        private Boolean _disposed;

        #endregion

        #region Destructor

        /// <summary>
        ///     Destructs the instance.
        /// </summary>
        ~Disposable()
        {
            Dispose( false );
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        ///     Dispose the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        ///     Releases all managed resources hold by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether the dispose method or the destructor is calling.</param>
        private void Dispose( Boolean disposing )
        {
            if ( disposing && !_disposed )
                Disposed();

            _disposed = true;
        }

        /// <summary>
        ///     Method invoked when the instance gets disposed.
        /// </summary>
        protected abstract void Disposed();

        #endregion
    }
}