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
        }
        
        [Header("Pool Settings")]
        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;
        
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
            
            InitializePools();
        }
        
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    objectPool.Enqueue(obj);
                }
                
                poolDictionary.Add(pool.tag, objectPool);
            }
        }
        
        public GameObject GetPooledObject(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist");
                return null;
            }
            
            if (poolDictionary[tag].Count == 0)
            {
                // Crear nuevo objeto si la pool está vacía
                Pool pool = pools.Find(p => p.tag == tag);
                if (pool != null)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    return obj;
                }
                return null;
            }
            
            GameObject objectToSpawn = poolDictionary[tag].Dequeue();
            objectToSpawn.SetActive(true);
            
            return objectToSpawn;
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
            poolDictionary[tag].Enqueue(obj);
        }
        
        public void ClearPool(string tag)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                foreach (GameObject obj in poolDictionary[tag])
                {
                    Destroy(obj);
                }
                poolDictionary[tag].Clear();
            }
        }
        
        public void ClearAllPools()
        {
            foreach (var pool in poolDictionary)
            {
                ClearPool(pool.Key);
            }
        }
    }
}
