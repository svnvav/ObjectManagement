using System.Collections.Generic;
using UnityEngine;

namespace Catlike.ObjectManagement
{
    public static class ShapeBehaviourPool<T> where T : ShapeBehaviour, new()
    {
        private static Stack<T> _stack = new Stack<T>();

        public static T Get()
        {
            if (_stack.Count > 0)
            {
                var instance = _stack.Pop();
#if UNITY_EDITOR
                instance.IsReclaimed = false;
#endif
                return instance;
            }

#if UNITY_EDITOR
            return ScriptableObject.CreateInstance<T>();
#else
		    return new T();
#endif
        }

        public static void Reclaim(T instance)
        {
#if UNITY_EDITOR
            instance.IsReclaimed = true;
#endif
            _stack.Push(instance);
        }
    }
}