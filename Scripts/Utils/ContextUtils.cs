using System;
using System.Threading;
using UnityEngine;

namespace Framework
{
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
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
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
    }
}
