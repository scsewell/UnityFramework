using UnityEngine;

namespace Framework.Internal
{
    public class AsyncCoroutineRunner : ComponentSingleton<AsyncCoroutineRunner>
    {
        protected override void Awake()
        {
            base.Awake();

            gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}