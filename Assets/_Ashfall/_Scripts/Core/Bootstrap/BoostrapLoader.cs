using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Ashfall._Scripts.Core.Bootstrap
{
    /// <summary>
    /// Entry point of the game. Loads the Persistent scene first,
    /// then hands off to GameManager to decide what comes next.
    /// This scene unloads itself after Persistent is ready.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string persistentSceneName = "Persistent";

        private IEnumerator Start()
        {
            // Load Persistent additively — keeps it alive for the entire session
            var op = SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);
            yield return op;

            // Set Persistent as active scene so new objects spawn there by default
            var persistent = SceneManager.GetSceneByName(persistentSceneName);
            SceneManager.SetActiveScene(persistent);

            // Unload Bootstrap — job done, never needed again
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }
    }
}