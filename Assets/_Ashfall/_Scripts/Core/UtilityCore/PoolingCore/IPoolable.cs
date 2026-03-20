using UnityEngine;

namespace _Ashfall._Scripts.Core.UtilityCore.PoolingCore
{
    /// <summary>
    /// Defines the basic contract for an object that can be pooled.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called immediately after the object is retrieved from the pool (SetActive is true).
        /// Use this to reset state (health, ammo, position, velocity).
        /// </summary>
        void OnGetFromPool();

        /// <summary>
        /// Called immediately before the object is returned to the pool (SetActive becomes false).
        /// Use this to clean up effects, stop coroutines, or reset rigidbodies.
        /// </summary>
        void OnReturnToPool();
    }
    
    /// <summary>
    /// Defines a poolable object that needs a reference to its owning pool.
    /// This allows the object to return itself (e.g., bullet hits wall -> returns to pool).
    /// </summary>
    /// <typeparam name="T">The specific type of the MonoBehaviour.</typeparam>
    public interface IPoolableWithInit<T> : IPoolable where T : MonoBehaviour, IPoolableWithInit<T>
    {
        /// <summary>
        /// Initializes the object with a reference to the pool that created it.
        /// Automatically called by the Pooler when creating or getting the object.
        /// </summary>
        /// <param name="pool">The pool instance managing this object.</param>
        void InitPool(Pooler<T> pool);
    }
}