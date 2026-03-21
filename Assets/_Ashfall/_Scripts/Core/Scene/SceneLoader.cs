using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Ashfall._Scripts.Core.Scene
{
    /// <summary>
    /// Handles additive zone loading and unloading.
    /// Sits in Persistent scene — never destroyed.
    /// All zone transitions go through here.
    /// </summary>
    public class SceneLoader : MonoBehaviour, ISceneService
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        private string _currentZone;

        private void Awake()
        {
            ServiceLocator.ServiceLocator.Register<ISceneService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.ServiceLocator.Unregister<ISceneService>();
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Transition from current zone to a new zone.
        /// Handles fade, unload old, load new, reposition player.
        /// </summary>
        public void TransitionTo(string newZoneName, string fromZoneId = null)
        {
            if (_currentZone == newZoneName) return;
            StartCoroutine(TransitionRoutine(newZoneName, fromZoneId));
        }

        // ── Internal ──────────────────────────────────────────────────────

        private IEnumerator TransitionRoutine(string newZoneName, string fromZoneId)
        {
            // 1. Fade out
            yield return FadeRoutine(0f, 1f);

            // 2. Unload current zone
            if (!string.IsNullOrEmpty(_currentZone))
            {
                var unload = SceneManager.UnloadSceneAsync(_currentZone);
                yield return unload;
            }

            // 3. Load new zone additive
            var load = SceneManager.LoadSceneAsync(newZoneName, LoadSceneMode.Additive);
            yield return load;

            _currentZone = newZoneName;

            // 4. Reposition player at correct SpawnPoint
            RepositionPlayer(fromZoneId);

            // 5. Fade in
            yield return FadeRoutine(1f, 0f);
        }

        private void RepositionPlayer(string fromZoneId)
        {
            // Find SpawnPoint in new zone that matches where player came from
            var spawnPoints = FindObjectsByType<ZoneSpawnPoint>(FindObjectsInactive.Exclude);

            ZoneSpawnPoint target = null;

            // Try to find matching entry point first
            foreach (var sp in spawnPoints)
            {
                if (sp.fromZoneId == fromZoneId)
                {
                    target = sp;
                    break;
                }
            }

            // Fallback to default spawn if no match found
            if (target == null)
            {
                foreach (var sp in spawnPoints)
                {
                    if (sp.isDefault)
                    {
                        target = sp;
                        break;
                    }
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"[SceneLoader] No SpawnPoint found in {_currentZone}");
                return;
            }

            // TODO: Teleport player — no destroy/respawn needed
            // if (ServiceLocator.ServiceLocator.TryGet<IPlayerService>(out var player))
            //     player.TeleportTo(target.transform.position);
        }

        private IEnumerator FadeRoutine(float from, float to)
        {
            if (fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            fadeCanvasGroup.alpha = from;
            fadeCanvasGroup.gameObject.SetActive(true);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;

            // Hide overlay when fully transparent
            if (to <= 0f)
                fadeCanvasGroup.gameObject.SetActive(false);
        }
    }
}