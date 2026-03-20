using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Ashfall._Scripts.Core.UtilityCore.PoolingCore
{
    /// <summary>
    /// A high-performance, generic object pooler capable of managing multiple prefab types.
    /// Optimized for O(1) lookup performance using internal dictionary mapping.
    /// </summary>
    /// <typeparam name="TPoolableObject">The type of object to pool, must implement IPoolableWithInit.</typeparam>
    public class Pooler<TPoolableObject> : MonoBehaviour where TPoolableObject : MonoBehaviour, IPoolableWithInit<TPoolableObject>
    {
        [Header("Configuration")]
        [SerializeField] private List<PoolEntry> poolEntries;
        [SerializeField, Min(0)] private int poolSizePerType = 5;
        [SerializeField] private bool expandable = true;
        [SerializeField] private int maxPoolSizePerType = 20;
        
        // Internal storage for inactive objects
        private readonly Dictionary<string, Queue<TPoolableObject>> _pools = new();
        
        // Map ID to Prefab for instantiation
        private readonly Dictionary<string, TPoolableObject> _prefabMap = new();
        
        // Track all active objects for validation
        private readonly HashSet<TPoolableObject> _allActive = new();

        // [OPTIMIZATION] Maps active instances directly to their Pool ID for O(1) lookup during Return operations.
        private readonly Dictionary<TPoolableObject, string> _instanceToIdMap = new();

        // [OPTIMIZATION] Caches the count of active objects per type to avoid O(N) iteration checks.
        private readonly Dictionary<string, int> _activeCountMap = new();
        
        /// <summary>
        /// Gets the total number of active objects currently tracked by the pooler.
        /// </summary>
        public int TotalActiveCount => _allActive.Count;

        /// <summary>
        /// Checks if this pooler manages only a single type of object.
        /// </summary>
        public bool IsSingleType => poolEntries is { Count: 1 };

        [Serializable]
        public class PoolEntry
        {
            public string id;
            public TPoolableObject prefab;
        }

        private void Awake()
        {
            InitializePools();
        }

        /// <summary>
        /// Initializes the pools based on the configuration list.
        /// Pre-instantiates objects to meet the 'poolSizePerType' requirement.
        /// </summary>
        private void InitializePools()
        {
            if (poolEntries == null || poolEntries.Count == 0)
            {
                Debug.LogWarning("[Pooler] No pool entries defined!");
                return;
            }

            foreach (var entry in poolEntries)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    Debug.LogWarning("[Pooler] Pool entry has empty ID, skipping...");
                    continue;
                }

                if (!entry.prefab)
                {
                    Debug.LogWarning($"[Pooler] Pool entry '{entry.id}' has no prefab, skipping...");
                    continue;
                }

                if (_prefabMap.ContainsKey(entry.id))
                {
                    Debug.LogWarning($"[Pooler] Duplicate ID '{entry.id}', skipping...");
                    continue;
                }

                // Register type
                _prefabMap[entry.id] = entry.prefab;
                _pools[entry.id] = new Queue<TPoolableObject>();
                _activeCountMap[entry.id] = 0;

                // Pre-fill pool
                for (int i = 0; i < poolSizePerType; i++)
                {
                    CreateNewInstance(entry.id, entry.prefab);
                }
            }
        }

        private TPoolableObject CreateNewInstance(string id, TPoolableObject prefab)
        {
            TPoolableObject obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            
            // Map instance to ID immediately upon creation
            _instanceToIdMap[obj] = id;
            
            _pools[id].Enqueue(obj);
            return obj;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Retrieves an object from the pool by its unique ID.
        /// </summary>
        /// <param name="id">The pool ID defined in the inspector.</param>
        /// <param name="position">Target position.</param>
        /// <param name="rotation">Target rotation.</param>
        /// <returns>An active object instance, or null if the pool is empty and not expandable.</returns>
        // ReSharper disable Unity.PerformanceAnalysis
        public TPoolableObject Get(string id, Vector3 position, Quaternion rotation)
        {
            if (string.IsNullOrEmpty(id) || !_pools.ContainsKey(id))
            {
                Debug.LogError($"[Pooler] Invalid or missing pool ID: {id}");
                return null;
            }

            TPoolableObject obj;
            
            if (_pools[id].Count > 0)
            {
                obj = _pools[id].Dequeue();
            }
            else
            {
                if (!expandable)
                {
                    Debug.LogWarning($"[Pooler] Pool '{id}' exhausted and not expandable!");
                    return null;
                }

                // Check limit using cached count O(1)
                if (_activeCountMap[id] >= maxPoolSizePerType)
                {
                    Debug.LogWarning($"[Pooler] Pool '{id}' reached max size ({maxPoolSizePerType})!");
                    return null;
                }

                // Create new instance (automatically registered in maps)
                obj = CreateNewInstance(id, _prefabMap[id]);
                
                // Dequeue immediately because CreateNewInstance enqueues it
                _pools[id].Dequeue();
            }

            // Update State
            _allActive.Add(obj);
            _activeCountMap[id]++; 

            // Initialize and Transform
            obj.InitPool(this);
            obj.OnGetFromPool();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
            
            return obj;
        }

        /// <summary>
        /// Retrieves an object from a single-type pool. 
        /// Warning: Do not use this for multi-type pools.
        /// </summary>
        public TPoolableObject GetFromPool(Vector3 position, Quaternion rotation)
        {
            if (IsSingleType)
            {
                // Iterate once to get the first key (safe for single type)
                foreach (var key in _prefabMap.Keys) return Get(key, position, rotation);
            }
            
            Debug.LogWarning("[Pooler] GetFromPool() is designed for single-type pools. Use Get(id) or GetRandom() instead.");
            return GetRandom(position, rotation);
        }

        /// <summary>
        /// Retrieves a random object type from the available pools.
        /// </summary>
        public TPoolableObject GetRandom(Vector3 position, Quaternion rotation)
        {
            if (_prefabMap.Count == 0) return null;

            // Note: Creating a list from Keys generates garbage. 
            // Ideally, cache the keys list in InitializePools if calling GetRandom frequently.
            var keys = new List<string>(_prefabMap.Keys); 
            string randomId = keys[Random.Range(0, keys.Count)];
            return Get(randomId, position, rotation);
        }

        /// <summary>
        /// Returns an active object to the pool.
        /// Automatically identifies the correct pool type using O(1) lookup.
        /// </summary>
        /// <param name="obj">The object instance to return.</param>
        // ReSharper disable Unity.PerformanceAnalysis
        public void Return(TPoolableObject obj)
        {
            if (obj == null) return;
            
            // Fast check if managed by this pooler
            if (!_allActive.Contains(obj)) return;

            // [OPTIMIZATION] O(1) ID Lookup
            if (!_instanceToIdMap.TryGetValue(obj, out string typeId))
            {
                Debug.LogError($"[Pooler] Object '{obj.name}' is active but not found in ID map! Destroying as fallback.");
                _allActive.Remove(obj);
                Destroy(obj.gameObject);
                return;
            }

            // Update State
            _allActive.Remove(obj);
            if (_activeCountMap.ContainsKey(typeId))
            {
                _activeCountMap[typeId]--;
                if (_activeCountMap[typeId] < 0) _activeCountMap[typeId] = 0;
            }

            // Reset transform hierarchy
            if (this && transform)
            {
                obj.transform.SetParent(transform, false);
            }
            
            // Trigger interface callback
            obj.OnReturnToPool();
            obj.gameObject.SetActive(false);
            
            // Enqueue back to specific pool
            _pools[typeId].Enqueue(obj);
        }

        /// <summary>
        /// Alias for Return(obj).
        /// </summary>
        public void ReturnToPool(TPoolableObject obj) => Return(obj);

        /// <summary>
        /// Forces all currently active objects managed by this pooler to return to the pool.
        /// </summary>
        public void ReturnAll()
        {
            if (_allActive.Count == 0) return;
            
            // Create snapshot to avoid collection modification errors during iteration
            var snapshot = new List<TPoolableObject>(_allActive);
            foreach (var obj in snapshot)
            {
                Return(obj);
            }
        }

        public void ReturnAllToPool() => ReturnAll();

        /// <summary>
        /// Returns the count of active objects for a specific type ID.
        /// </summary>
        public int GetActiveCount(string id)
        {
            return _activeCountMap.TryGetValue(id, out int count) ? count : 0;
        }
        
        /// <summary>
        /// Executes an action on every currently active object.
        /// </summary>
        public void ForEachActive(Action<TPoolableObject> action)
        {
            if (action == null) return;
            foreach (var obj in _allActive) action(obj);
        }

        /// <summary>
        /// Executes an action on every currently active object of a specific type.
        /// </summary>
        public void ForEachActiveOfType(string id, Action<TPoolableObject> action)
        {
            if (action == null) return;

            foreach (var obj in _allActive)
            {
                // Efficiently check ID without string comparison on name
                if (_instanceToIdMap.TryGetValue(obj, out string objId) && objId == id)
                {
                    action(obj);
                }
            }
        }

        public bool HasType(string id) => _pools.ContainsKey(id);

        public List<string> GetAllTypeIds() => new List<string>(_prefabMap.Keys);
    }
}