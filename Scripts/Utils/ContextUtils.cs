using System;
using System.Threading;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Utilities for managing threading contexts.
    /// </summary>
    public static class ContextUtils
    {
        /// <summary>
        /// The unity main thread.
        /// </summary>
        public static int UnityThreadId { get; private set; }

        /// <summary>
        /// The main synchronization context.
        /// </summary>
        public static SynchronizationContext UnitySynchronizationContext { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
            UnitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Runs an action on the main thread context.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void RunOnUnityScheduler(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (SynchronizationContext.Current == UnitySynchronizationContext)
            {
                action();
            }
            else
            {
                UnitySynchronizationContext.Post(_ => action(), null);
            }
        }

        /// <summary>
        /// Checks if the caller is the main thread.
        /// </summary>
        /// <returns>True if on the main thread.</returns>
        public static bool EnsureMainThread([CallerMemberName] string callerName = "")
        {
            if (Thread.CurrentThread.ManagedThreadId != UnityThreadId)
            {
                Debug.LogError($"{callerName} should only be called by the main thread!");
                return false;
            }
            return true;
        }
    }
}
