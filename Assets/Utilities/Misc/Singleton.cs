namespace Volpi.Entertaiment.SDK.Utilities
{
    using UnityEngine;

    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        [SerializeField] private bool _isGlobalSingleton = true;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject(typeof(T).Name);
                        _instance = singleton.AddComponent<T>();

                        Debug.Log("[Singleton] Created new instance of " + typeof(T));

                        if ((_instance as Singleton<T>)._isGlobalSingleton)
                        {
                            DontDestroyOnLoad(singleton);
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;

                if (_isGlobalSingleton)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[Singleton] Duplicate instance of " + typeof(T) + " found, destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }
}