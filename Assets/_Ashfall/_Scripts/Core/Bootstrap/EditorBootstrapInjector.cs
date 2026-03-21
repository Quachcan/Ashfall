using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Ashfall._Scripts.Core.Bootstrap
{
    /// <summary>
    /// Ensures Bootstrap always runs first, even when playing directly
    /// from a Zone scene in the Editor. Stripped from production builds
    /// since Build Settings scene order handles it there.
    /// </summary>
    public static class EditorBootstrapInjector
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InjectBootstrap()
        {
            // Already in Bootstrap — no need to inject
            // if (SceneManager.GetActiveScene().name == "Bootstrap") return;

            // Load Bootstrap first — it will chain-load Persistent
            // SceneManager.LoadScene("BoostrapScene", LoadSceneMode.Single);
        }
#endif
    }
}