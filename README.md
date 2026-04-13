# README — Technical Overview

> Game **2.5D Action Platformer / Metroidvania / Soulslike** — Unity 6.4 URP, C#, PC & Mobile

---

## 📋 Tech Stack

| Mục | Chi tiết |
| --- | --- |
| Engine | Unity 6.4 (URP) |
| Ngôn ngữ | C# |
| Platform | PC & Mobile (Android/iOS) |
| Rendering | 2.5D — 3D visuals, 2D gameplay plane |

**Packages chính:**

| Package | Mục đích |
| --- | --- |
| DOTween (Demigiant) | UI animation, tween toàn bộ |
| Cinemachine | Camera follow, confiner theo zone |
| TextMesh Pro | Render text |
| Unity Input System | Input PC + Mobile, virtual joystick |
| Odin Inspector | Inspector tooling, SerializeField helpers |

---

## 🎮 Game Design

### Nhân vật
- **1 nhân vật duy nhất** — chọn giới tính (Nam/Nữ) khi bắt đầu
- Playstyle thay đổi theo **vũ khí trang bị**, không theo class cố định

### Weapon Types
| Vũ khí | Playstyle | Block/Parry |
|--------|-----------|-------------|
| Kiếm một tay | Melee cân bằng, combo 3 đòn | ✅ Có |
| Song kiếm | Tốc độ cao, combo dài | ❌ Chỉ dodge |
| Gậy phép | Ranged magic, AOE, burst | ❌ Chỉ dodge |
| Cung | Ranged vật lý, trap | ❌ Chỉ dodge |

### Combat (Soulslike)
- Không hồi HP tự động — chỉ hồi tại Grace Point hoặc dùng item
- **Stamina** — dùng cho dash, sprint, attack, block
- **Posture/Stagger** (Sekiro-style) — tăng khi nhận đòn, đầy → stagger → finishing blow window
- **Block/Parry** — giữ = block (tốn stamina), bấm đúng timing = parry (stagger enemy)
- **Damage formula** — `ATK × (100 / 100 + DEF)` (Dark Souls style)

---

## 🗂️ Cấu trúc Project

```
Assets/_Ashfall/_Scripts/
├── Core/
│   ├── EventCore/          SO-based pub/sub (Event<T>, VoidEvent, EventHub)
│   ├── StateMachineCore/   Generic FSM (StateMachine<TEnum>, IState)
│   ├── PoolingCore/        Object pool (Pooler<T>, IPoolable)
│   ├── ServiceLocator/     DI container
│   ├── AudioCore/          AudioManager, AudioBank, AudioCue SO
│   ├── Scene/              SceneLoader, ZoneSpawnPoint, ISceneService
│   ├── UICore/             BaseUi, UiButtonFX, DynamicCanvasScaler
│   └── Bootstrap/          BootstrapLoader, EditorBootstrapInjector
│
├── Gameplay/
│   ├── Player/
│   │   ├── PlayerController.cs     Root MonoBehaviour, owns FSM + systems
│   │   ├── PlayerContext.cs        Shared data container cho tất cả states
│   │   ├── PlayerStats.cs          ScriptableObject — config per weapon type
│   │   ├── PlayerInputHandler.cs   New Input System wrapper
│   │   ├── PlayerState.cs          Enum: Idle/Run/Jump/Fall/Dash/Attack/...
│   │   ├── AnimHash.cs             Cached Animator parameter hashes
│   │   ├── StaminaSystem.cs        Pure C# — ticked bởi PlayerController
│   │   ├── AnimatorEventBridge.cs  Forward Animator events → PlayerController
│   │   └── States/                 1 file per state, implement IState
│   │
│   └── Combat/
│       ├── IHittable.cs            Interface — anything that receives hits
│       ├── ICombatStats.cs         Interface — exposes ATK/MAG/DEF
│       ├── HitData.cs              SO — per-attack config (damage, knockback...)
│       ├── HitboxWeapon.cs         Weapon collider, enable/disable per frame
│       ├── HurtboxController.cs    Implements IHittable, routes hits
│       ├── HurtboxZone.cs          Zone marker collider (Head/Torso/Legs)
│       ├── HealthSystem.cs         Pure C# — HP management
│       ├── PostureSystem.cs        Pure C# — Sekiro-style posture/stagger
│       ├── DamageCalculator.cs     Static utility — damage formula
│       └── DummyEnemy.cs           Test target (xóa khi có Enemy thật)
│
├── Systems/                        (planned)
│   ├── SkillSystem/
│   ├── QuestSystem/
│   ├── Economy/
│   └── Farming/
│
└── UI/
    ├── HUD/                        (planned)
    ├── Menus/                      (planned)
    └── Shared/                     BaseUi, DOTween helpers
```

---

## 🏗️ Architecture — Core Patterns

### 1. Pure C# Systems (Performance)
Tránh nhiều MonoBehaviour.Update(). Systems là pure C#, ticked thủ công bởi owner:

```csharp
// PlayerController.Update() — 1 Update duy nhất
private void Update()
{
    _stamina.Tick(Time.deltaTime);   // pure C#
    _posture.Tick(Time.deltaTime);   // pure C#
    _fsm.Tick();
    UpdateAnimator();
}
```

**Rule:** Chỉ dùng MonoBehaviour khi cần Unity lifecycle hoặc Inspector serialization.

### 2. Generic StateMachine

```csharp
var fsm = new StateMachine<PlayerState>(states, PlayerState.Idle);
fsm.StateChanged += (from, to) => { };
fsm.Initialize();
fsm.ChangeState(PlayerState.Attack);
fsm.Tick();        // trong Update
fsm.FixedTick();   // trong FixedUpdate
```

### 3. EventHub — SO-based Pub/Sub

```csharp
// Subscribe (OnEnable):
eventHub.playerEvents.onPlayerDead.Subscribe(OnPlayerDead);

// Raise:
eventHub.playerEvents.onPlayerDead.Raise();

// Unsubscribe (OnDisable — bắt buộc):
eventHub.playerEvents.onPlayerDead.Unsubscribe(OnPlayerDead);
```

### 4. Service Locator

```csharp
// Register (Awake):
ServiceLocator.Register<IWalletService>(this);

// Consume:
ServiceLocator.Get<IWalletService>()?.AddAsh(100);

// Unregister (OnDestroy):
ServiceLocator.Unregister<IWalletService>();
```

### 5. Dependency Injection Pattern
Systems không dùng GetComponent — được inject bởi owner:

```csharp
// PlayerController.Awake()
_health  = new HealthSystem(stats.maxHp);
_stamina = new StaminaSystem(stats);
_posture = new PostureSystem(...);
_hurtbox.Initialize(_rb, _health, this, _posture); // inject
```

### 6. Object Pooler

```csharp
var fx = fxPool.GetFromPool(spawnPos, rotation);
// Object tự return về pool qua IPoolableWithInit<T>
```

---

## 🔗 Dependency Tree (Current)

```
PlayerController (MonoBehaviour)
├── PlayerInputHandler      reads New Input System
├── PlayerContext           shared data — passed to all states
├── StaminaSystem (C#)      ticked by PlayerController
├── HealthSystem (C#)       ticked by PlayerController, event-driven
├── PostureSystem (C#)      ticked by PlayerController
├── AnimatorOverrideController  random parry clips
└── StateMachine<PlayerState>
      └── States: Idle, Run, Jump, Fall, Dash, Attack,
                  CrouchIdle, CrouchWalk, Block, Parry,
                  GuardBreak, Dead

HurtboxController (MonoBehaviour)
├── IHittable               receives TakeHit() from HitboxWeapon
├── HealthSystem (injected)
├── PostureSystem (injected)
└── ICombatStats (injected) for DEF calculation

HitboxWeapon (MonoBehaviour, on weapon bone)
└── OnTriggerEnter → GetHittableRoot → IHittable.TakeHit()
      └── DamageCalculator.Calculate(ATK, DEF, type, zoneMultiplier)
```

---

## ⚙️ Scene Structure

```
Bootstrap (loads first, unloads after)
└── loads Persistent (additive, never unloads)
    ├── PlayerController    lives here — không destroy khi zone transition
    ├── AudioManager
    ├── SceneLoader         handles zone transitions + fade
    └── [Zone Scene]        loaded/unloaded additively per zone
```

---

## 📝 Coding Conventions

| Rule | Detail |
|------|--------|
| **Namespace** | `_Ashfall._Scripts.*` |
| **No prefix** | Không dùng prefix script (AF_, v.v.) |
| **Comments** | English |
| **Events** | Dùng `eventHub.*` — không dùng C# event trực tiếp (trừ internal systems) |
| **Services** | `ServiceLocator.Get<T>()` — luôn null-check (`?.`) |
| **StateMachine** | Mỗi state là 1 class riêng implement `IState` |
| **Unsubscribe** | Luôn unsubscribe EventHub trong `OnDisable` |
| **TimeScale** | Chỉ `GameManager` quản lý `Time.timeScale` |
| **Pooler** | Mọi object spawn nhiều lần đều dùng pool |
| **SO** | ItemData, SkillData, QuestData, EnemyData đều là ScriptableObject |
| **Physics** | `rb.linearVelocity` (Unity 6), `FindObjectsByType` dùng `FindObjectsInactive` |
| **Pure C#** | Systems không cần Unity lifecycle → pure C#, ticked bởi owner |

---

## 🌿 Branch Convention

| Branch | Mục đích |
|--------|----------|
| `main` | Production — stable |
| `develop` | Integration branch |
| `feature/<tên>` | Tính năng mới |
| `fix/<tên>` | Bug fix |
| `claude/<tên>` | AI-assisted development |


*Unity 6.4 · C# · PC & Mobile · DOTween · Cinemachine · TextMesh Pro · Odin Inspector*