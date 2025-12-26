using UnityEngine;
using System.Collections.Generic;

namespace SwyPhexLeague.Utilities
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }
        
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
            public Transform parent;
        }
        
        [Header("Pool Configuration")]
        public List<Pool> pools;
        
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Pool> poolConfigs;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolConfigs = new Dictionary<string, Pool>();
            
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = CreatePooledObject(pool);
                    objectPool.Enqueue(obj);
                }
                
                poolDictionary.Add(pool.tag, objectPool);
                poolConfigs.Add(pool.tag, pool);
            }
        }
        
        private GameObject CreatePooledObject(Pool pool)
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.SetActive(false);
            
            if (pool.parent != null)
            {
                obj.transform.SetParent(pool.parent);
            }
            else
            {
                obj.transform.SetParent(transform);
            }
            
            PooledObject pooledObj = obj.AddComponent<PooledObject>();
            pooledObj.poolTag = pool.tag;
            
            return obj;
        }
        
        public GameObject GetPooledObject(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist");
                return null;
            }
            
            if (poolDictionary[tag].Count == 0)
            {
                ExpandPool(tag);
            }
            
            GameObject obj = poolDictionary[tag].Dequeue();
            
            if (obj == null)
            {
                Debug.LogWarning($"Null object in pool {tag}, recreating");
                obj = RecreatePooledObject(tag);
            }
            
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            
            IPoolable[] poolables = obj.GetComponents<IPoolable>();
            foreach (IPoolable poolable in poolables)
            {
                poolable.OnSpawn();
            }
            
            return obj;
        }
        
        public GameObject GetPooledObject(string tag)
        {
            return GetPooledObject(tag, Vector3.zero, Quaternion.identity);
        }
        
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist");
                Destroy(obj);
                return;
            }
            
            obj.SetActive(false);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            
            if (poolConfigs[tag].parent != null)
            {
                obj.transform.SetParent(poolConfigs[tag].parent);
            }
            else
            {
                obj.transform.SetParent(transform);
            }
            
            IPoolable[] poolables = obj.GetComponents<IPoolable>();
            foreach (IPoolable poolable in poolables)
            {
                poolable.OnDespawn();
            }
            
            poolDictionary[tag].Enqueue(obj);
        }
        
        private void ExpandPool(string tag)
        {
            if (!poolConfigs.ContainsKey(tag))
            {
                Debug.LogWarning($"Cannot expand pool {tag}, config not found");
                return;
            }
            
            Pool pool = poolConfigs[tag];
            
            for (int i = 0; i < pool.size / 2; i++) // Expandir 50%
            {
                GameObject obj = CreatePooledObject(pool);
                poolDictionary[tag].Enqueue(obj);
            }
            
            Debug.Log($"Expanded pool {tag} by {pool.size / 2} objects");
        }
        
        private GameObject RecreatePooledObject(string tag)
        {
            if (!poolConfigs.ContainsKey(tag))
            {
                Debug.LogWarning($"Cannot recreate object for pool {tag}");
                return null;
            }
            
            return CreatePooledObject(poolConfigs[tag]);
        }
        
        public void ClearPool(string tag)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                while (poolDictionary[tag].Count > 0)
                {
                    GameObject obj = poolDictionary[tag].Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
                poolDictionary[tag].Clear();
            }
        }
        
        public void ClearAllPools()
        {
            foreach (string tag in poolDictionary.Keys)
            {
                ClearPool(tag);
            }
        }
        
        public int GetPoolSize(string tag)
        {
            return poolDictionary.ContainsKey(tag) ? poolDictionary[tag].Count : 0;
        }
        
        public interface IPoolable
        {
            void OnSpawn();
            void OnDespawn();
        }
        
        public class PooledObject : MonoBehaviour, IPoolable
        {
            public string poolTag;
            
            public void OnSpawn()
            {
                // Override en objetos específicos
            }
            
            public void OnDespawn()
            {
                // Override en objetos específicos
            }
            
            public void ReturnToPool()
            {
                ObjectPool.Instance?.ReturnToPool(poolTag, gameObject);
            }
        }
    }
}
