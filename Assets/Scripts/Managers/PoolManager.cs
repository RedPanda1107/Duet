using System.Collections.Generic;
using UnityEngine;

namespace Duet.Managers
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register a prefab for pooling
        /// </summary>
        public void RegisterPrefab(string key, GameObject prefab, int initialCount = 10)
        {
            if (!prefabs.ContainsKey(key))
            {
                prefabs[key] = prefab;
                pools[key] = new Queue<GameObject>();

                // Pre-instantiate objects
                for (int i = 0; i < initialCount; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.SetActive(false);
                    pools[key].Enqueue(obj);
                }
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        // Get an object from the pool
        public GameObject GetFromPool(string key)
        {
            if (pools.ContainsKey(key))
            {
                // Try to find a non-destroyed object in the pool
                while (pools[key].Count > 0)
                {
                    GameObject obj = pools[key].Dequeue();
                    if (obj == null)
                    {
                        // Skip destroyed entries
                        continue;
                    }
                    // record the key on the name for optional simpler ReturnToPool overloads
                    obj.name = key;
                    obj.SetActive(true);
                    return obj;
                }
            }

            if (prefabs.ContainsKey(key))
            {
                // Create new object if pool is empty or all pooled objects were destroyed
                GameObject obj = Instantiate(prefabs[key]);
                obj.name = key;
                obj.SetActive(true);
                return obj;
            }

            Debug.LogError($"Pool with key '{key}' not found!");
            return null;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void ReturnToPool(string key, GameObject obj)
        {
            if (obj == null)
                return;

            if (pools.ContainsKey(key))
            {
                obj.SetActive(false);
                pools[key].Enqueue(obj);
                OnReturnedToPool?.Invoke(key, obj);
            }
            else
            {
                Debug.LogError($"Pool with key '{key}' not found!");
                Destroy(obj);
            }
        }

        /// <summary>
        /// Convenience overload: return by reading object's name as key
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;
            string key = obj.name;
            ReturnToPool(key, obj);
        }
        
        // Event fired when an object is returned to a pool: (key, obj)
        public event System.Action<string, GameObject> OnReturnedToPool;
    }
}
