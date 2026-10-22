using UnityEngine;

namespace SmokeFree.Core
{
    /// <summary>
    /// Базовый класс для менеджеров, живущих всю сессию.
    /// Сознательно БЕЗ ленивого автосоздания: все менеджеры лежат в сцене 00_Bootstrap.
    /// Это убирает классический баг "синглтон заспавнился посреди геймплея и потерял конфиг".
    /// </summary>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isQuitting;

        public static T Instance => _isQuitting ? null : _instance;
        public static bool Exists => _instance != null && !_isQuitting;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            OnSingletonAwake();
        }

        /// <summary>Инициализация наследника. Вызывается один раз, только у выжившего инстанса.</summary>
        protected virtual void OnSingletonAwake() { }

        protected virtual void OnApplicationQuit() => _isQuitting = true;

        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
