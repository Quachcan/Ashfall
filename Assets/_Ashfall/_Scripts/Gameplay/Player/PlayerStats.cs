using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// ScriptableObject config for all player movement and physics stats.
    /// Create one asset per class (FighterStats, MageStats, etc.) and swap
    /// via the PlayerController Inspector — no prefab duplication needed.
    ///
    /// Create menu: Ashfall/Player/PlayerStats
    /// </summary>
    
    [CreateAssetMenu(menuName = "Ashfall/Player/PlayerStats", fileName = "PlayerStats_New", order = -1000)]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Top horizontal speed in units/s")]
        public float moveSpeed = 6f;
        
        [Tooltip("How fast the player reaches top speed (higher = snappier")]
        public float acceleration = 20f;
        
        [Tooltip("How fast the player decelerates when no input (higher = snappier stop")]
        public float deceleration = 25f;
        
        [Header("Jump")]
        [Tooltip("Upward velocity applied on jump")]
        public float jumpForce = 14f;
        
        [Tooltip("How long after walking off a ledge the player can still jump (seconds)")]
        public float coyoteTime      = 0.12f;
        
        [Tooltip("Max number of jump (1 = single, 2 = double jump")]
        public int maxJumps = 2;
        
        [Tooltip("Multiplier applied to gravity when falling (makes fall feel heavier)")]
        public float fallGravityMultiplier = 2.5f;
        
        [Tooltip("Multiplier applied when jump button is released early (shot hop")]
        public float lowJumpMultiplier = 2f;
        
        [Header("Dash")]
        [Tooltip("Speed of the dash in units/s")]
        public float dashSpeed = 18f;
        
        [Tooltip("Duration of the dash in seconds")]
        public float dashDuration = 0.18f;
        
        [Tooltip("Cooldown between dashes in seconds")]
        public float dashCooldown = 0.6f;
        
        [Header("Ground Check")]
        [Tooltip("LayerMask for ground detection")]
        public LayerMask groundLayer;
        
        [Tooltip("Offset from transform origin to the center of the ground-check sphere")]
        public Vector3 groundCheckOffset = new Vector3(0f, -0.05f, 0f);
        
        [Tooltip("Radius of the ground-check sphere")]
        public float groundCheckRadius = 0.25f;
        
        [Header("Wall check")]
        [Tooltip("LayerMask for Wall detection")]
        public LayerMask wallLayer;

        [Tooltip("Horizontal check offset for wall check ray")]
        public float wallCheckDistance = 0.4f;
    }
}