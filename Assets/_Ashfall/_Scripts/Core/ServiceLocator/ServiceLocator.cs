using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Ashfall._Scripts.Core.ServiceLocator
{
    /// <summary>
    /// A static, generic Service Locator for dependency injection without direct references.
    /// Systems register themselves on Awake, and consumers retrieve them anywhere via Get<T>().
    /// All warnings are Editor-only and stripped from production builds.
    /// </summary>
    public static class ServiceLocator
    {
        // Internal dictionary mapping Type → service instance
        private static readonly Dictionary<Type, object> _services = new();

        // ── Register ─────────────────────────────────────────────────────────

        /// <summary>
        /// Register a service. Call this in Awake() of the implementing MonoBehaviour.
        /// Warns if the same interface is registered twice — usually a setup mistake.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);

            if (_services.ContainsKey(type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[ServiceLocator] '{type.Name}' is already registered. " +
                                 $"Overwriting with new instance: {service}");
#endif
            }

            _services[type] = service;
        }

        // ── Get ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieve a registered service.
        /// Returns null and logs a warning if the service has not been registered yet.
        /// Always null-check the result or use TryGet() for optional services.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var service))
                return service as T;

#if UNITY_EDITOR
            Debug.LogWarning($"[ServiceLocator] '{type.Name}' is not registered. " +
                             $"Make sure the provider calls Register<{type.Name}>() in Awake().");
#endif
            return null;
        }

        /// <summary>
        /// Try to retrieve a service without triggering a warning.
        /// Use this for optional services that may or may not be present.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (_services.TryGetValue(type, out var raw) && raw is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        // ── Unregister ────────────────────────────────────────────────────────

        /// <summary>
        /// Unregister a service. Call this in OnDestroy() of the implementing MonoBehaviour
        /// to prevent stale references after the object is destroyed.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);

            if (!_services.Remove(type))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[ServiceLocator] Tried to unregister '{type.Name}' " +
                                 $"but it was not registered.");
#endif
            }
        }

        // ── Utility ───────────────────────────────────────────────────────────

        /// <summary>
        /// Check if a service is currently registered.
        /// Useful before Get() when a service is truly optional.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
            => _services.ContainsKey(typeof(T));

        /// <summary>
        /// Clear all registered services.
        /// Call this when doing a full scene reload or during test teardown.
        /// Not needed during normal zone transitions — only wipe on full game restart.
        /// </summary>
        public static void ClearAll()
        {
#if UNITY_EDITOR
            Debug.Log($"[ServiceLocator] Cleared all {_services.Count} registered services.");
#endif
            _services.Clear();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: log all currently registered services to the Console.
        /// Useful for debugging missing or duplicate registrations.
        /// </summary>
        public static void DebugPrintAll()
        {
            if (_services.Count == 0)
            {
                Debug.Log("[ServiceLocator] No services registered.");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[ServiceLocator] {_services.Count} registered service(s):");
            foreach (var kvp in _services)
                sb.AppendLine($"  • {kvp.Key.Name} → {kvp.Value}");

            Debug.Log(sb.ToString());
        }
#endif
    }
}