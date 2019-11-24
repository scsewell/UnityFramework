using System;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class that contains disposal boiler plate code.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        /// <summary>
        /// Indicates if this instance has been disposed.
        /// </summary>
        public bool Disposed { get; private set; } = false;

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // make sure this instance will not run its finalizer
            // since we have manually cleaned up this instance.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Try to dispose resources if the finalizer is called. This should
        /// not be the case, since the finalizer should not run if dispose was 
        /// called.
        /// </summary>
        ~Disposable()
        {
            Debug.LogError($"An instance of type \"{GetType().FullName}\" was not disposed!");
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        /// <param name="disposing">If true managed resources should be cleaned up.</param>
        private void Dispose(bool disposing)
        {
            if (!Disposed && CanDispose())
            {
                OnDispose(disposing);
                Disposed = true;
            }
        }

        /// <summary>
        /// Logs an error if this instance has been disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (Disposed)
            {
                Debug.LogError($"A disposed instance of type \"{GetType().FullName}\" was accessed!");
            }
        }

        /// <summary>
        /// Checks if this instance can be disposed.
        /// </summary>
        protected virtual bool CanDispose() => true;

        /// <summary>
        /// Cleans up resources held by this instance.
        /// </summary>
        /// <param name="disposing">If true this call is not executing on the finalizer thread.</param>
        protected abstract void OnDispose(bool disposing);
    }
}