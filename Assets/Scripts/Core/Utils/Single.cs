using System;
using System.Reflection;

namespace Assets.Scripts.Util
{
    public class Single<T> where T : class, new()
    {
        private static T _mInstance;

        public static T Instance
        {
            get
            {
                if (_mInstance == null)
                {
                    _mInstance = Activator.CreateInstance<T>();
                }
                return _mInstance;
            }
        }

        public static void DestroyInstance()
        {
            if (_mInstance != null)
            {
                MethodInfo methord = typeof(T).GetMethod("OnDestroy");
                if (methord != null)
                {
                    methord.Invoke(_mInstance, null);
                }
                _mInstance = null;
            }
        }
    }
}