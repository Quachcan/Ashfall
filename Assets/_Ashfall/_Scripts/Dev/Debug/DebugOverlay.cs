using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Optional interface: provides current state name to DebugOverlay.
/// </summary>
public interface IDebugStateProvider
{
    string GetDebugStateName();
}

/// <summary>
/// Optional interface: provides current sub-state name to DebugOverlay.
/// </summary>
public interface IDebugSubStateProvider
{
    string GetDebugSubStateName();
}

/// <summary>
/// In-game debug overlay. Displays FPS, scene, state, position, velocity.
/// All sections collapsible. Buttons to toggle overlay and gizmos from Inspector.
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    // ── Providers ─────────────────────────────────────────────────────────

    [BoxGroup("Providers", centerLabel: true)]
    [LabelText("State Provider")]
    [Tooltip("MonoBehaviour implementing IDebugStateProvider")]
    public MonoBehaviour stateProvider;

    [BoxGroup("Providers")]
    [LabelText("SubState Provider")]
    [Tooltip("MonoBehaviour implementing IDebugSubStateProvider (optional — falls back to stateProvider)")]
    public MonoBehaviour subStateProvider;

    // ── Target ────────────────────────────────────────────────────────────

    [FoldoutGroup("Target", expanded: true)]
    [LabelText("Transform")]
    public Transform target;

    [FoldoutGroup("Target")]
    [ShowIf("@target != null")]
    [BoxGroup("Target/Runtime Info")]
    [DisplayAsString, LabelText("Position")]
    [ShowInInspector, HideInEditorMode]
    private string TargetPosition => target
        ? $"{target.position.x:0.00},  {target.position.y:0.00},  {target.position.z:0.00}"
        : "—";

    [BoxGroup("Target/Runtime Info")]
    [DisplayAsString, LabelText("Velocity")]
    [ShowInInspector, HideInEditorMode]
    private string TargetVelocity
    {
        get
        {
            if (!target) return "—";
            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                var v = rb.linearVelocity;
                return $"{v.magnitude:0.00} m/s   ({v.x:0.00}, {v.y:0.00}, {v.z:0.00})";
            }
            return "No Rigidbody";
        }
    }

    [BoxGroup("Target/Runtime Info")]
    [DisplayAsString, LabelText("State")]
    [ShowInInspector, HideInEditorMode]
    private string RuntimeState => TryGetStateName() ?? "—";

    [BoxGroup("Target/Runtime Info")]
    [DisplayAsString, LabelText("Sub-State")]
    [ShowInInspector, HideInEditorMode]
    private string RuntimeSubState => TryGetSubStateName() ?? "—";

    [BoxGroup("Target/Runtime Info")]
    [DisplayAsString, LabelText("FPS")]
    [ShowInInspector, HideInEditorMode]
    private string RuntimeFps => $"{_fps:0.0}  |  TimeScale: {Time.timeScale:0.##}";

    // ── Target Zone ───────────────────────────────────────────────────────

    [FoldoutGroup("Target Zone", expanded: false)]
    [LabelText("Center")]
    public Transform targetZoneCenter;

    [FoldoutGroup("Target Zone")]
    [LabelText("Radius"), Min(0f)]
    public float targetZoneRadius = 3f;

    [FoldoutGroup("Target Zone")]
    [LabelText("Color")]
    public Color targetZoneColor = new Color(0.2f, 0.8f, 1f, 0.9f);

    // ── Overlay Settings ──────────────────────────────────────────────────

    [FoldoutGroup("Overlay Settings", expanded: true)]
    [HorizontalGroup("Overlay Settings/Toggles")]
    [ToggleLeft, LabelText("Show GUI")]
    public bool showGUI = true;

    [HorizontalGroup("Overlay Settings/Toggles")]
    [ToggleLeft, LabelText("Show Gizmos")]
    public bool showGizmos = true;

    [FoldoutGroup("Overlay Settings")]
    [LabelText("Font Size"), Range(10, 80)]
    public int guiFontSize = 26;

    [FoldoutGroup("Overlay Settings")]
    [LabelText("Area Position")]
    public Vector2 guiAreaPos = new Vector2(10, 10);

    [FoldoutGroup("Overlay Settings")]
    [LabelText("Area Size")]
    public Vector2 guiAreaSize = new Vector2(420, 260);

    // ── Gizmos Style ──────────────────────────────────────────────────────

    [FoldoutGroup("Gizmos Style", expanded: false)]
    [LabelText("Target Color")]
    public Color gizmoTargetColor = Color.yellow;

    [FoldoutGroup("Gizmos Style")]
    [LabelText("Velocity Color")]
    public Color gizmoVelocityColor = Color.cyan;

#if UNITY_EDITOR
    [FoldoutGroup("Gizmos Style")]
    [ToggleLeft, LabelText("Show Handles (Editor)")]
    public bool showHandles = true;

    [FoldoutGroup("Gizmos Style")]
    [ShowIf("showHandles")]
    [LabelText("Handles Label Color")]
    public Color handlesLabelColor = new Color(1f, 1f, 0.2f, 1f);

    [FoldoutGroup("Gizmos Style")]
    [ShowIf("showHandles")]
    [LabelText("Arrow Size"), Min(0.1f)]
    public float handlesArrowSize = 2f;
#endif

    // ── Hotkeys ───────────────────────────────────────────────────────────

    [FoldoutGroup("Hotkeys", expanded: false)]
    [LabelText("Toggle GUI")]
    public Key toggleGUIKey = Key.F2;

    [FoldoutGroup("Hotkeys")]
    [LabelText("Toggle Gizmos")]
    public Key toggleGizmosKey = Key.F3;

#if UNITY_EDITOR
    [FoldoutGroup("Hotkeys")]
    [LabelText("Toggle Handles (Editor)")]
    public Key toggleHandlesKey = Key.F4;
#endif

    // ── Inspector Buttons ─────────────────────────────────────────────────

    [TitleGroup("Quick Actions")]
    [HorizontalGroup("Quick Actions/Buttons")]
    [Button("Toggle GUI", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 0.4f)]
    private void ToggleGUI() => showGUI = !showGUI;

    [HorizontalGroup("Quick Actions/Buttons")]
    [Button("Toggle Gizmos", ButtonSizes.Medium), GUIColor(0.4f, 0.6f, 1f)]
    private void ToggleGizmos() => showGizmos = !showGizmos;

    [HorizontalGroup("Quick Actions/Buttons")]
    [Button("Reset Styles", ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.3f)]
    private void ResetStyles()
    {
        _labelStyle  = null;
        _headerStyle = null;
    }

    [TitleGroup("Quick Actions")]
    [Button("Print State to Console", ButtonSizes.Large), GUIColor(0.9f, 0.9f, 0.3f)]
    private void PrintStateToConsole()
    {
        string state    = TryGetStateName()    ?? "None";
        string subState = TryGetSubStateName() ?? "None";
        string pos      = target ? $"{target.position}" : "No Target";
        Debug.Log($"[DebugOverlay] State={state} | Sub={subState} | Pos={pos} | FPS={_fps:0.0}");
    }

    // ── Private Runtime ───────────────────────────────────────────────────

    private GUIStyle _labelStyle;
    private GUIStyle _headerStyle;

    private float _fps;
    private float _accum;
    private int   _frames;
    private float _fpsTimer;

    // ── Unity Lifecycle ───────────────────────────────────────────────────

    private void Update()
    {
        // Hotkeys — New Input System
        // Note: Keyboard[Key] indexer throws ArgumentOutOfRangeException for keys like F2-F4
        // because their enum values exceed the internal keymap array size.
        // Use InputSystem.GetKey() via the helper below instead.
        if (WasPressedThisFrame(toggleGUIKey))     showGUI    = !showGUI;
        if (WasPressedThisFrame(toggleGizmosKey))  showGizmos = !showGizmos;
#if UNITY_EDITOR
        if (WasPressedThisFrame(toggleHandlesKey)) showHandles = !showHandles;
#endif

        // FPS rolling average (update every 0.5s)
        _accum  += Time.unscaledDeltaTime;
        _frames++;
        _fpsTimer += Time.unscaledDeltaTime;
        if (_fpsTimer >= 0.5f)
        {
            _fps      = _frames / _accum;
            _fpsTimer = 0f;
            _accum    = 0f;
            _frames   = 0;
        }
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        // Lazy-init styles (OnGUI is the only safe place to create GUIStyle)
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = guiFontSize,
                richText = true,
                normal   = { textColor = Color.white }
            };
        }

        if (_headerStyle == null)
        {
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = guiFontSize + 2,
                fontStyle = FontStyle.Bold,
                richText  = true,
                normal    = { textColor = new Color(1f, 0.85f, 0.2f) }
            };
        }

        GUILayout.BeginArea(new Rect(guiAreaPos, guiAreaSize), GUI.skin.box);

        GUILayout.Label("<b>⚙ Debug Overlay</b>", _headerStyle);

        // Scene & performance
        GUILayout.Label($"Scene: <b>{SceneManager.GetActiveScene().name}</b>", _labelStyle);
        GUILayout.Label($"FPS: <b>{_fps:0.0}</b>  |  TimeScale: <b>{Time.timeScale:0.##}</b>", _labelStyle);

        // State
        string stateName = TryGetStateName();
        if (!string.IsNullOrEmpty(stateName))
            GUILayout.Label($"State: <b>{stateName}</b>", _labelStyle);

        string subStateName = TryGetSubStateName();
        if (!string.IsNullOrEmpty(subStateName))
            GUILayout.Label($"Sub: <b>{subStateName}</b>", _labelStyle);

        // Target
        if (target)
        {
            Vector3 p = target.position;
            GUILayout.Label($"Pos: <b>{p.x:0.00}, {p.y:0.00}, {p.z:0.00}</b>", _labelStyle);

            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                var v = rb.linearVelocity;
                GUILayout.Label($"Vel: <b>{v.magnitude:0.00}</b> m/s  ({v.x:0.00}, {v.y:0.00}, {v.z:0.00})", _labelStyle);
            }
        }

        // Hotkey hints
        GUILayout.Space(4);
#if UNITY_EDITOR
        GUILayout.Label($"<color=#aaaaaa>[F2] GUI  [F3] Gizmos  [F4] Handles</color>", _labelStyle);
#else
        GUILayout.Label($"<color=#aaaaaa>[F2] GUI  [F3] Gizmos</color>", _labelStyle);
#endif
        GUILayout.EndArea();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (target)
        {
            Gizmos.color = gizmoTargetColor;
            Gizmos.DrawWireSphere(target.position, 0.4f);

            if (target.TryGetComponent<Rigidbody>(out var rb))
            {
                Gizmos.color = gizmoVelocityColor;
                Vector3 v = rb.linearVelocity;
                Gizmos.DrawLine(target.position, target.position + v);
            }
        }

        if (targetZoneCenter && targetZoneRadius > 0f)
        {
            Gizmos.color = targetZoneColor;
            Gizmos.DrawWireSphere(targetZoneCenter.position, targetZoneRadius);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || !target || !showHandles) return;

        Handles.color = handlesLabelColor;
        Handles.ArrowHandleCap(
            0,
            target.position,
            Quaternion.LookRotation(
                Camera.current
                    ? Camera.current.transform.position - target.position
                    : Vector3.forward),
            handlesArrowSize,
            EventType.Repaint);

        Handles.Label(
            target.position + Vector3.up * 1.2f,
            $"Target\n{target.position}",
            new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = handlesLabelColor } });
    }
#endif

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Safe key press check using InputSystem.GetKey().
    /// Avoids ArgumentOutOfRangeException from the Keyboard[Key] indexer
    /// which doesn't support all Key enum values (e.g. F2-F15).
    /// </summary>
    private static bool WasPressedThisFrame(Key key)
    {
        try
        {
            var ctrl = InputSystem.FindControl($"<Keyboard>/{key.ToString().ToLower()}");
            return ctrl is UnityEngine.InputSystem.Controls.ButtonControl btn && btn.wasPressedThisFrame;
        }
        catch { return false; }
    }

    private string TryGetStateName()
    {
        if (stateProvider is IDebugStateProvider sp)
        {
            try   { return sp.GetDebugStateName(); }
            catch { return "[State Error]"; }
        }
        return null;
    }

    private string TryGetSubStateName()
    {
        if (subStateProvider is IDebugSubStateProvider ssp)
        {
            try   { return ssp.GetDebugSubStateName(); }
            catch { return "[SubState Error]"; }
        }

        // Fallback: stateProvider might implement both interfaces
        if (stateProvider is IDebugSubStateProvider ssp2)
        {
            try   { return ssp2.GetDebugSubStateName(); }
            catch { return "[SubState Error]"; }
        }

        return null;
    }
}