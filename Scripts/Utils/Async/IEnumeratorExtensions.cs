using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class that contains extention methods used to make coroutines
    /// async/await compatible.
    /// </summary>
    public static class IEnumeratorExtensions
    {
        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSeconds instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForEndOfFrame instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForFixedUpdate instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitForSecondsRealtime instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitUntil instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter GetAwaiter(this WaitWhile instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static SimpleCoroutineAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        private static SimpleCoroutineAwaiter GetAwaiterReturnVoid(object instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                InstructionWrappers.ReturnVoid(awaiter, instruction)));
            return awaiter;
        }

        private static SimpleCoroutineAwaiter<T> GetAwaiterReturnSelf<T>(T instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<T>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                InstructionWrappers.ReturnSelf(awaiter, instruction)));
            return awaiter;
        }

        public static SimpleCoroutineAwaiter<UnityEngine.Object> GetAwaiter(this ResourceRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<UnityEngine.Object>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                InstructionWrappers.ResourceRequest(awaiter, instruction)));
            return awaiter;
        }

        public static SimpleCoroutineAwaiter<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<AssetBundle>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                InstructionWrappers.AssetBundleCreateRequest(awaiter, instruction)));
            return awaiter;
        }

        public static SimpleCoroutineAwaiter<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
        {
            var awaiter = new SimpleCoroutineAwaiter<UnityEngine.Object>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                InstructionWrappers.AssetBundleRequest(awaiter, instruction)));
            return awaiter;
        }

        public static SimpleCoroutineAwaiter<object> GetAwaiter(this IEnumerator coroutine)
        {
            var awaiter = new SimpleCoroutineAwaiter<object>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                new CoroutineWrapper<object>(coroutine, awaiter).Run()));
            return awaiter;
        }

        public static SimpleCoroutineAwaiter<T> GetAwaiter<T>(this IEnumerator<T> coroutine)
        {
            var awaiter = new SimpleCoroutineAwaiter<T>();
            ContextUtils.RunOnUnityScheduler(() => AsyncCoroutineRunner.Instance.StartCoroutine(
                new CoroutineWrapper<T>(coroutine, awaiter).Run()));
            return awaiter;
        }

        private static class InstructionWrappers
        {
            public static IEnumerator ReturnVoid(SimpleCoroutineAwaiter awaiter, object instruction)
            {
                yield return instruction;
                awaiter.Complete(null);
            }

            public static IEnumerator ReturnSelf<T>(SimpleCoroutineAwaiter<T> awaiter, T instruction)
            {
                yield return instruction;
                awaiter.Complete(instruction, null);
            }

            public static IEnumerator AssetBundleCreateRequest(SimpleCoroutineAwaiter<AssetBundle> awaiter, AssetBundleCreateRequest instruction)
            {
                yield return instruction;
                awaiter.Complete(instruction.assetBundle, null);
            }

            public static IEnumerator AssetBundleRequest(SimpleCoroutineAwaiter<UnityEngine.Object> awaiter, AssetBundleRequest instruction)
            {
                yield return instruction;
                awaiter.Complete(instruction.asset, null);
            }

            public static IEnumerator ResourceRequest(SimpleCoroutineAwaiter<UnityEngine.Object> awaiter, ResourceRequest instruction)
            {
                yield return instruction;
                awaiter.Complete(instruction.asset, null);
            }
        }

        public class SimpleCoroutineAwaiter : INotifyCompletion
        {
            private bool m_isDone = false;
            private Exception m_exception = null;
            private Action m_continuation = null;

            public bool IsCompleted => m_isDone;

            public void GetResult()
            {
                if (m_exception != null)
                {
                    ExceptionDispatchInfo.Capture(m_exception).Throw();
                }
            }

            public void Complete(Exception e)
            {
                m_isDone = true;
                m_exception = e;

                if (m_continuation != null)
                {
                    ContextUtils.RunOnUnityScheduler(m_continuation);
                }
            }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                m_continuation = continuation;
            }
        }

        public class SimpleCoroutineAwaiter<T> : INotifyCompletion
        {
            private bool m_isDone = false;
            private Exception m_exception = null;
            private Action m_continuation = null;
            private T m_result = default;

            public bool IsCompleted => m_isDone;

            public T GetResult()
            {
                if (m_exception != null)
                {
                    ExceptionDispatchInfo.Capture(m_exception).Throw();
                }
                return m_result;
            }

            public void Complete(T result, Exception e)
            {
                m_isDone = true;
                m_exception = e;
                m_result = result;

                if (m_continuation != null)
                {
                    ContextUtils.RunOnUnityScheduler(m_continuation);
                }
            }

            void INotifyCompletion.OnCompleted(Action continuation)
            {
                m_continuation = continuation;
            }
        }

        private class CoroutineWrapper<T>
        {
            private readonly SimpleCoroutineAwaiter<T> m_awaiter;
            private readonly Stack<IEnumerator> m_processStack;

            public CoroutineWrapper(IEnumerator coroutine, SimpleCoroutineAwaiter<T> awaiter)
            {
                m_processStack = new Stack<IEnumerator>();
                m_processStack.Push(coroutine);
                m_awaiter = awaiter;
            }

            public IEnumerator Run()
            {
                while (true)
                {
                    var topWorker = m_processStack.Peek();

                    bool isDone;

                    try
                    {
                        isDone = !topWorker.MoveNext();
                    }
                    catch (Exception e)
                    {
                        // The IEnumerators we have in the process stack do not tell us the
                        // actual names of the coroutine methods but it does tell us the objects
                        // that the IEnumerators are associated with, so we can at least try
                        // adding that to the exception output.
                        var objectTrace = GenerateObjectTrace(m_processStack);

                        if (objectTrace.Any())
                        {
                            m_awaiter.Complete(default, new Exception(GenerateObjectTraceMessage(objectTrace), e));
                        }
                        else
                        {
                            m_awaiter.Complete(default, e);
                        }

                        yield break;
                    }

                    if (isDone)
                    {
                        m_processStack.Pop();

                        if (m_processStack.Count == 0)
                        {
                            m_awaiter.Complete((T)topWorker.Current, null);
                            yield break;
                        }
                    }

                    // We could just yield return nested IEnumerator's here but we choose to do
                    // our own handling here so that we can catch exceptions in nested coroutines
                    // instead of just top level coroutine.
                    if (topWorker.Current is IEnumerator)
                    {
                        m_processStack.Push((IEnumerator)topWorker.Current);
                    }
                    else
                    {
                        // Return the current value to the unity engine so it can handle things like
                        // WaitForSeconds, WaitToEndOfFrame, etc.
                        yield return topWorker.Current;
                    }
                }
            }

            private string GenerateObjectTraceMessage(List<Type> objTrace)
            {
                var result = new StringBuilder();

                foreach (var objType in objTrace)
                {
                    if (result.Length != 0)
                    {
                        result.Append(" -> ");
                    }

                    result.Append(objType.ToString());
                }

                result.AppendLine();
                return "Unity Coroutine Object Trace: " + result.ToString();
            }

            private static List<Type> GenerateObjectTrace(IEnumerable<IEnumerator> enumerators)
            {
                var objTrace = new List<Type>();

                foreach (var enumerator in enumerators)
                {
                    var field = enumerator.GetType().GetField("$this", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                    if (field == null)
                    {
                        continue;
                    }

                    var obj = field.GetValue(enumerator);

                    if (obj == null)
                    {
                        continue;
                    }

                    var objType = obj.GetType();

                    if (!objTrace.Any() || objType != objTrace.Last())
                    {
                        objTrace.Add(objType);
                    }
                }

                objTrace.Reverse();
                return objTrace;
            }
        }
    }
}
