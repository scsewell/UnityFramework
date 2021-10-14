using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// Utilities for doing common reflection operations.
    /// </summary>
    public class ReflectionUtils
    {
        /// <summary>
        /// Searches all assemblies for all types which derive from the given type.
        /// </summary>
        /// <typeparam name="T">The type of which to get all derived types.</typeparam>
        /// <returns>A new array of types.</returns>
        public static Type[] GetTypesDerivedFrom<T>()
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom<T>()
#else
            return GetAllTypes().Where(type => typeof(T).IsAssignableFrom(type))
#endif
                .ToArray();
        }
        
        static Type[] GetAllTypes()
        {
            var allTypes = new ConcurrentBag<Type>();

            Parallel.ForEach(AppDomain.CurrentDomain.GetAssemblies(), assembly =>
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException typeLoadException)
                {
                    types = typeLoadException.Types;
                }

                foreach (var type in types)
                {
                    allTypes.Add(type);
                }
            });

            return allTypes.ToArray();
        }
    }
}
