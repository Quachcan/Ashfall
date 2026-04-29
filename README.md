# Ashfall

> A 2.5D action platformer with Soulslike combat — built in Unity 6.4 URP

---

## Overview

Ashfall is a **2.5D Soulslike** where playstyle is defined by the weapon you carry, not a fixed class.
The project is a vertical slice demo focused on demonstrating polished combat systems and clean code architecture.

**Target scope:** 1 playable area · 2 weapon types · 2 enemy variants · 1 boss

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 6.4 (URP) |
| Language | C# |
| Platform | PC (Windows) |
| Rendering | 2.5D — 3D visuals on a 2D gameplay plane (Z-axis locked Rigidbody) |

**Key packages:**

| Package | Purpose |
|---|---|
| Unity Input System | Keyboard + Gamepad input, action map |
| Cinemachine | Camera follow, zone confinement |
| Odin Inspector | Editor tooling, custom Inspector layouts |
| DOTween | UI animations and tweening |
| TextMesh Pro | Text rendering |

---

## Gameplay

### Character
Single character with gender selection (Male / Female) at game start.
No fixed class — playstyle changes entirely based on the equipped weapon.

### Weapons

| Weapon | Style | Combo | Block & Parry |
|--------|-------|-------|---------------|
| **Sword** | Balanced melee | 3-hit combo | ✅ Block + Parry |
| **Bow** | Ranged physical | Tap = quick shot · Hold = charged shot | ❌ Dodge only |

### Combat Systems
- **Stamina** — shared resource for attacking, blocking, and dashing
- **Posture / Stagger** (Sekiro-inspired) — fills on every hit received; full posture → stagger → finishing blow window
- **Block / Parry** — hold to block (costs stamina); press at the right moment to parry (staggers attacker)
- **Damage formula** — `ATK × (100 / (100 + DEF))` inspired by Dark Souls scaling
- **Crit system** — per-attack crit rate and multiplier defined in `AttackData` ScriptableObjects
- **Knockback** — strong hits lock player input and apply a physics impulse

---

## Architecture

### Folder Structure

```
Assets/_Ashfall/_Scripts/
├── Core/
│   ├── StateMachineCore/   Generic FSM (StateMachine<TEnum>, IState)
│   └── PoolingCore/        Generic object pool (Pooler<T>, IPoolable)
│
└── Gameplay/
    ├── Player/
    │   ├── PlayerController.cs       Root MonoBehaviour — owns FSM and all systems
    │   ├── PlayerContext.cs          Shared data container passed to every state
    │   ├── PlayerStats.cs            ScriptableObject — movement, jump, dash, stamina config
    │   ├── PlayerInputHandler.cs     New Input System adapter — states never call InputSystem directly
    │   ├── AnimHash.cs               Cached Animator parameter hashes (avoids per-frame string lookup)
    │   ├── StaminaSystem.cs          Pure C# — ticked manually by PlayerController
    │   ├── HealthSystem.cs           Pure C# — HP management, event-driven
    │   ├── PostureSystem.cs          Pure C# — Sekiro-style stagger meter
    │   └── States/                   One file per FSM state, each implements IState
    │
    ├── Combat/
    │   ├── AttackData.cs             ScriptableObject — per-hit config (damage, crit, knockback, animation)
    │   ├── HitboxWeapon.cs           Weapon trigger collider — enabled/disabled via Animator events
    │   ├── HurtboxController.cs      Receives hits (IHittable) — routes to Health, Posture, Rigidbody
    │   ├── DamageCalculator.cs       Static utility — damage formula and flat damage
    │   ├── ArrowProjectile.cs        Rigidbody projectile — spawned by BowAttackState on release
    │   └── IHittable.cs / ICombatStats.cs   Interfaces decoupling attacker from defender
    │
    └── Weapons/
        ├── WeaponData.cs             ScriptableObject — pure combat numbers (no visual assets)
        ├── WeaponVisuals.cs          ScriptableObject — prefab, AnimatorOverrideController, arrow prefab
        ├── WeaponVisualRegistry.cs   Maps WeaponType enum → WeaponVisuals at runtime
        └── WeaponHandler.cs          Manages equip flow — swaps animator and model on weapon change
```

### Key Design Patterns

**1 — Finite State Machine**

All player behaviour is modelled as an FSM. Each state is an isolated class implementing `IState` — no shared mutable state, no `if/else` chains in `Update`.

```csharp
// PlayerController.cs
private StateMachine<PlayerState> BuildFSM()
{
    _states = new Dictionary<PlayerState, IState>
    {
        { PlayerState.Idle,       new PlayerIdleState(this, _ctx)       },
        { PlayerState.Attack,     new PlayerAttackState(this, _ctx)     },
        { PlayerState.BowAttack,  new PlayerBowAttackState(this, _ctx)  },
        { PlayerState.Block,      new PlayerBlockState(this, _ctx)      },
        // ...
    };
    return new StateMachine<PlayerState>(_states, PlayerState.Idle);
}
```

**2 — Pure C# Systems**

Performance-sensitive systems have no `MonoBehaviour` overhead — they are plain C# classes ticked by a single owner.

```csharp
// PlayerController.Update() — one Update call drives everything
private void Update()
{
    _stamina.Tick(Time.deltaTime);
    _posture.Tick(Time.deltaTime);
    _fsm.Tick();
    UpdateAnimator();
}
```

**3 — Data / Visual Separation**

`WeaponData` holds only combat numbers and is always in memory.
`WeaponVisuals` holds prefabs and animation assets, loaded only when a weapon is equipped.
The two are linked by a `WeaponType` enum — no direct SO cross-reference.

```
WeaponData_Sword  ──┐
                    ├── WeaponType.Sword ──► WeaponVisualRegistry ──► WeaponVisuals_Sword
WeaponData_Bow    ──┘                                                  (prefab, animator, arrow)
```

**4 — AnimatorOverrideController Pattern**

One `AC_Player_Base` Animator Controller contains all states with placeholder clips.
Each weapon has a pre-built `AnimatorOverrideController` asset in the Editor that swaps
only the clips relevant to that weapon. `WeaponHandler` creates a **runtime copy** on equip
so that clip mutations (e.g. random parry clip selection) never affect the shared project asset.

```csharp
// WeaponHandler.SwapAnimator()
OverrideController = new AnimatorOverrideController(source.runtimeAnimatorController);
source.GetOverrides(overrides);
OverrideController.ApplyOverrides(overrides);   // copy — asset untouched
_animator.runtimeAnimatorController = OverrideController;
```

**5 — Dependency Injection (manual)**

Systems are created and injected by their owner — no `GetComponent` chains, no singletons.

```csharp
// PlayerController.Awake()
_health  = new HealthSystem(stats.maxHp);
_stamina = new StaminaSystem(stats);
_posture = new PostureSystem(stats.maxPosture, ...);
_hurtbox.Initialize(_rb, _health, this, _posture);   // inject all dependencies
```

### System Dependency Map

```
PlayerController
├── PlayerInputHandler     — New Input System adapter
├── PlayerContext          — shared data bag for all states
├── StaminaSystem (C#)     — ticked every Update
├── HealthSystem (C#)      — event: OnDeath → FSM.ChangeState(Dead)
├── PostureSystem (C#)     — event: OnStagger → stagger state
├── WeaponHandler          — equip flow, AnimatorOverrideController
└── StateMachine<PlayerState>
      └── 14 states: Idle · Run · Jump · Fall · Dash
                     Attack · BowAttack
                     CrouchIdle · CrouchWalk
                     Block · Parry · GuardBreak
                     Knockback · Dead

HurtboxController (on Player root)
├── IHittable              — receives TakeHit() from HitboxWeapon / ArrowProjectile
├── HealthSystem (ref)     — applies final damage
├── PostureSystem (ref)    — applies posture damage
└── ICombatStats (ref)     — reads DEF for damage reduction

HitboxWeapon (on weapon bone)
└── OnTriggerEnter → HurtboxController.TakeHit(AttackData, hitPoint, direction, attacker)
      └── DamageCalculator.Calculate(ATK, DEF, DamageType)
```

---

## Coding Conventions

| Convention | Rule |
|---|---|
| Namespace | `_Ashfall._Scripts.*` |
| Language | English only in source files |
| State pattern | One class per state, implements `IState` |
| Input | States read from `PlayerInputHandler` — never call `InputSystem` directly |
| Animator | All parameter hashes cached in `AnimHash.cs` — no runtime string hashing |
| Systems | No `MonoBehaviour` unless Unity lifecycle or Inspector serialization is required |
| Null safety | Null-guard at state `Enter()` — log error and transition to safe state |
| Physics | `rb.linearVelocity` (Unity 6 API), Z-axis frozen on all character Rigidbodies |

---

## Project Status

### Implemented ✅
- Full player FSM (14 states)
- Melee combo system (3-hit, per-hit AttackData, input buffering)
- Bow ranged attack (charge mechanic, quick shot / charged shot, ArrowProjectile)
- Block · Parry · Guard Break pipeline
- Knockback state
- Dash with i-frames
- Crouch (idle + walk)
- Double jump + coyote time
- Stamina system (regeneration, depletion, exhaustion)
- Health system (event-driven death)
- Posture / Stagger system (Sekiro-inspired)
- Damage pipeline (Physical / Magic / True damage types, crit, knockback)
- Weapon equip system (WeaponData + WeaponVisuals + AnimatorOverrideController)
- Weapon hot-swap at runtime

### In Progress 🔄
- Enemy AI (patrol, detection, attack)
- Level design — first area blockout
- HUD (HP bar, stamina bar, posture indicator)

### Planned 📋
- Boss fight
- Grace Point respawn system
- Camera polish (combat zoom, hit shake)
- VFX + SFX integration
- Game feel pass (hit stop, screen shake, particle on hit)
