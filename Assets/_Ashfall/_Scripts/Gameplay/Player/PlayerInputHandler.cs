using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Reads raw input from Unity's New Input System and exposes clean,
    /// consumed flags for the Player FSM states to read each frame.
    /// 
    /// Pattern: states never call InputSystem directly — they only read
    /// the processed values here. This makes states input-agnostic
    /// (same states work for keyboard, gamepad, and mobile virtual buttons).
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Raw Axis ──────────────────────────────────────────────────────

        /// <summary>Horizontal input value in range [-1, 1].</summary>
        public float MoveX { get; private set; }

        // ── Buffered / Consumed Flags ─────────────────────────────────────
        // These are set true when the button is pressed and consumed (set false)
        // by the state that handles them. Buffering prevents missed inputs.

        /// <summary>True for one frame when Jump is pressed (consumed by JumpState).</summary>
        public bool JumpPressed  { get; private set; }

        /// <summary>True while Jump button is held (used for variable jump height).</summary>
        public bool JumpHeld     { get; private set; }

        /// <summary>True for one frame when Dash is pressed (consumed by DashState).</summary>
        public bool DashPressed  { get; private set; }

        /// <summary>True for one frame when Attack is pressed (consumed by AttackState).</summary>
        public bool AttackPressed { get; private set; }

        // ── Input Actions ─────────────────────────────────────────────────

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _attackAction;

        private void Awake()
        {
            _playerInput  = GetComponent<PlayerInput>();

            // Grab actions by name — these must match the InputActionAsset
            _moveAction   = _playerInput.actions["Move"];
            _jumpAction   = _playerInput.actions["Jump"];
            _dashAction   = _playerInput.actions["Dash"];
            _attackAction = _playerInput.actions["Attack"];
        }

        private void OnEnable()
        {
            _jumpAction.performed   += OnJumpPerformed;
            _jumpAction.canceled    += OnJumpCanceled;
            _dashAction.performed   += OnDashPerformed;
            _attackAction.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            _jumpAction.performed   -= OnJumpPerformed;
            _jumpAction.canceled    -= OnJumpCanceled;
            _dashAction.performed   -= OnDashPerformed;
            _attackAction.performed -= OnAttackPerformed;
        }

        private void Update()
        {
            // Read move axis every frame
            MoveX = _moveAction.ReadValue<Vector2>().x;
        }

        // ── Consumed API (called by states) ───────────────────────────────

        /// <summary>
        /// Consume the JumpPressed flag — call this inside the state that handles it
        /// so it won't be processed again next frame.
        /// </summary>
        public void ConsumeJump()    => JumpPressed   = false;

        /// <summary>Consume the DashPressed flag.</summary>
        public void ConsumeDash()    => DashPressed   = false;

        /// <summary>Consume the AttackPressed flag.</summary>
        public void ConsumeAttack()  => AttackPressed = false;

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnJumpPerformed(InputAction.CallbackContext ctx)  => JumpPressed  = true;
        private void OnJumpCanceled(InputAction.CallbackContext ctx)   => JumpHeld     = false;
        private void OnDashPerformed(InputAction.CallbackContext ctx)  => DashPressed  = true;
        private void OnAttackPerformed(InputAction.CallbackContext ctx)=> AttackPressed = true;

        private void OnJumpButtonHeld(InputAction.CallbackContext ctx) => JumpHeld = true;

        // Note: JumpHeld is set via polling in Update for more reliable hold detection
        private void LateUpdate()
        {
            JumpHeld = _jumpAction.IsPressed();
        }
    }
}