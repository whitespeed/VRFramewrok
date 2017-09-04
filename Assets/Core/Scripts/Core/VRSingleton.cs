using UnityEngine;

namespace Framework
{
    public abstract class VRSingleton<T> : MonoBehaviour where T : VRSingleton<T>
    {
        private static readonly string PREFIX = @"[Singleton] " + typeof(T);
        private static T _instance;
        //private static readonly object _lock = new object();
        protected static bool isAppQuitting = false;
        public static bool IsAppQuitting {
            get { return isAppQuitting; }
        }

        public static T Instance
        {
            get
            {
                if (IsAppQuitting)
                {
                    LogError("already destroyed on application quit.");
                    return null;
                }

                if (_instance == null)
                {
                    LogError("should call Create() in VRApplication.CreateSingleton");
                }

                return _instance;
            }

        }
        public static T Create()
        {
            if (_instance == null)
            {
                isAppQuitting = false;
                GameObject singleton = new GameObject();
                _instance = singleton.AddComponent<T>();
                singleton.name = PREFIX;
                DontDestroyOnLoad(singleton);
                Instance.OnInitialize();
                Log("Created.");
            }
            else
            {
                LogWarning("already created, not need to create again.");
            }
            return Instance;
        }

        public static void Destroy()
        {
            Instance.OnUninitialize();
            GameObject.Destroy(_instance.gameObject);
            _instance = null;
            isAppQuitting = true;
            Log("Destroy.");
        }


        public abstract void OnInitialize();

        public abstract void OnUninitialize();


        protected static void Log(string message)
        {
            Debug.LogFormat(@"{0} {1}", PREFIX, message);
        }
        protected static void LogWarning(string message)
        {
            Debug.LogWarningFormat(@"{0} {1}", PREFIX, message);
        }
        protected static void LogError(string message)
        {
            Debug.LogErrorFormat(@"{0} {1}", PREFIX, message);
        }
    }

}