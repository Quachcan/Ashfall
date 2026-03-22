using UnityEngine;
using UnityEngine.InputSystem;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Reads raw input from Unity's New Input System and exposes clean,
    /// consumed flags for the Player FSM states to read each frame.
    ///
    /// Pattern: states never call InputSystem directly — they only read
    /// processed values here. This makes states input-agnostic
    /// (same states work for keyboard, gamepad, and mobile virtual buttons).
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Raw Axis ──────────────────────────────────────────────────────

        /// <summary>Horizontal input value in range [-1, 1].</summary>
        public float MoveX { get; private set; }

        // ── Jump ──────────────────────────────────────────────────────────

        /// <summary>
        /// True if jump was pressed recently and not yet consumed.
        /// Stays true for JumpBufferTime seconds to prevent missed inputs.
        /// </summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True while Jump button is held (variable jump height).</summary>
        public bool JumpHeld    { get; private set; }

        // ── Dash / Sprint ─────────────────────────────────────────────────

        /// <summary>True for one frame when Dash button is pressed. Consumed by DashState.</summary>
        public bool DashPressed { get; private set; }

        /// <summary>True while Dash button is physically held down.</summary>
        public bool DashHeld    { get; private set; }

        /// <summary>
        /// True while sprinting. Set by DashState when dash ends with button still held.
        /// Public setter allows DashState to activate sprint directly.
        /// </summary>
        public bool SprintHeld  { get; set; }

        // ── Attack ────────────────────────────────────────────────────────

        /// <summary>True for one frame when Attack is pressed. Consumed by AttackState.</summary>
        public bool AttackPressed { get; private set; }

        // ── Block / Parry ─────────────────────────────────────────────────

        /// <summary>True while Block button is physically held.</summary>
        public bool BlockHeld     { get; private set; }

        /// <summary>
        /// True for one frame when Block is pressed.
        /// Used to detect parry window (tap) vs block (hold).
        /// </summary>
        public bool BlockPressed  { get; private set; }

        /// <summary>
        /// True for one frame when Block button is released.
        /// Used by BlockState to detect hold duration and decide block vs parry.
        /// </summary>
        public bool BlockReleased { get; private set; }

        // ── Crouch ────────────────────────────────────────────────────────

        /// <summary>
        /// True for one frame when Crouch button is pressed (toggle).
        /// Consumed by Idle/Run/Crouch states.
        /// </summary>
        public bool CrouchToggled { get; private set; }

        /// <summary>
        /// Current crouch toggle state — true = crouching, false = standing.
        /// Flips each press. Can be force-set by states (e.g. on jump to exit crouch).
        /// </summary>
        public bool IsCrouching { get; set; }

        // ── Jump Buffer ───────────────────────────────────────────────────

        [SerializeField] private float jumpBufferTime = 0.1f;
        private float _jumpBufferTimer;

        // ── Input Actions ─────────────────────────────────────────────────

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _attackAction;
        private InputAction _crouchAction;
        private InputAction _blockAction;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _playerInput  = GetComponent<PlayerInput>();
            _moveAction   = _playerInput.actions["Move"];
            _jumpAction   = _playerInput.actions["Jump"];
            _dashAction   = _playerInput.actions["Dash"];
            _attackAction = _playerInput.actions["Attack"];
            _crouchAction = _playerInput.actions["Crouch"];
            _blockAction  = _playerInput.actions["Block"];
        }

        private void OnEnable()
        {
            _jumpAction.performed   += OnJumpPerformed;
            _jumpAction.canceled    += OnJumpCanceled;
            _dashAction.performed   += OnDashPerformed;
            _dashAction.canceled    += OnDashCanceled;
            _attackAction.performed += OnAttackPerformed;
            _crouchAction.performed += OnCrouchPerformed;
            _blockAction.performed  += OnBlockPerformed;
            _blockAction.canceled   += OnBlockCanceled;
        }

        private void OnDisable()
        {
            _jumpAction.performed   -= OnJumpPerformed;
            _jumpAction.canceled    -= OnJumpCanceled;
            _dashAction.performed   -= OnDashPerformed;
            _dashAction.canceled    -= OnDashCanceled;
            _attackAction.performed -= OnAttackPerformed;
            _crouchAction.performed -= OnCrouchPerformed;
            _blockAction.performed  -= OnBlockPerformed;
            _blockAction.canceled   -= OnBlockCanceled;
        }

        private void Update()
        {
            MoveX = _moveAction.ReadValue<Vector2>().x;

            // Tick jump buffer
            if (_jumpBufferTimer > 0f)
            {
                _jumpBufferTimer -= Time.deltaTime;
                if (_jumpBufferTimer <= 0f)
                    JumpPressed = false;
            }

            // Clear one-frame flags
            BlockPressed  = false;
            BlockReleased = false;
        }

        private void LateUpdate()
        {
            JumpHeld = _jumpAction.IsPressed();
        }

        #region Consume API (call by states)

        /// <summary>Consume JumpPressed and clear buffer timer.</summary>
        public void ConsumeJump()
        {
            JumpPressed      = false;
            _jumpBufferTimer = 0f;
        }

        /// <summary>Consume DashPressed flag.</summary>
        public void ConsumeDash()    => DashPressed    = false;

        /// <summary>Consume SprintHeld flag.</summary>
        public void ConsumeSprint()  => SprintHeld     = false;

        /// <summary>Consume AttackPressed flag.</summary>
        public void ConsumeAttack()  => AttackPressed  = false;

        /// <summary>Consume CrouchToggled flag (keep IsCrouching state).</summary>
        public void ConsumeCrouch()  => CrouchToggled  = false;
        
        public void ConsumeBlock()    => BlockPressed  = false;

        /// <summary>
        /// Clear crouch entirely — resets both IsCrouching and CrouchToggled.
        /// Call when an action forcibly exits crouch (e.g. jump, death).
        /// </summary>
        public void ClearCrouch()
        {
            IsCrouching   = false;
            CrouchToggled = false;
        }

        public void ClearBlock()
        {
            BlockHeld    = false;
            BlockPressed  = false;
            BlockReleased = false;
        }

        #endregion

        #region Call backs

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            JumpPressed      = true;
            _jumpBufferTimer = jumpBufferTime;
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)  => JumpHeld = false;

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            DashHeld    = true;
            DashPressed = true;
            SprintHeld  = false;
        }

        private void OnDashCanceled(InputAction.CallbackContext ctx)
        {
            DashHeld   = false;
            SprintHeld = false;
        }

        private void OnAttackPerformed(InputAction.CallbackContext ctx) => AttackPressed = true;

        private void OnCrouchPerformed(InputAction.CallbackContext ctx)
        {
            IsCrouching   = !IsCrouching;
            CrouchToggled = true;
        }

        private void OnBlockPerformed(InputAction.CallbackContext ctx)
        {
            BlockHeld    = true;
            BlockPressed = true;
        }

        private void OnBlockCanceled(InputAction.CallbackContext ctx)
        {
            BlockHeld     = false;
            BlockReleased = true;
        }

        #endregion
    }
}