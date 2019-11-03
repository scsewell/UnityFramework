using UnityEngine;

namespace Framework
{
    /// <summary>
    /// When converting an async task to a coroutine, we need an object for unity 
    /// to run the coroutine on. We use this singleton instance to do that.
    /// </summary>
    internal class AsyncCoroutineRunner : ComponentSingleton<AsyncCoroutineRunner>
    {
        protected override void Awake()
        {
            base.Awake();

            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}