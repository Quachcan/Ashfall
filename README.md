README — Technical Overview
> Game **2.5D Action Platformer / Metroidvania / Soulslike** — Unity URP, C#, PC & Mobile
> 

---

## 📋 Tech Stack

| Mục | Chi tiết |
| --- | --- |
| Engine | Unity 2022 LTS (URP) |
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

---

## 🗂️ Cấu trúc Project (dự kiến)

```
Assets/Ashfall/Scripts/
├── Core/
│   ├── EventHub/           SO-based pub/sub — hệ thống event chính
│   ├── StateMachine/       Generic FSM dùng lại cho Player, Enemy, Boss
│   ├── Pooling/            Object pool thống nhất
│   ├── ServiceLocator/     DI container — inject các service
│   └── SaveSystem/         JSON + PlayerPrefs
│
├── Gameplay/
│   ├── Player/             Controller, Stats, Combat, Skills
│   ├── Enemy/              Base Enemy AI, Boss FSM
│   ├── GracePoint/         Checkpoint, Respawn, Teleport
│   ├── World/              Zone transition, Metroidvania gate
│   └── Items/              ItemData SO, Pickup, Inventory
│
├── Systems/
│   ├── SkillSystem/        Skill tree, Active/Passive
│   ├── StatSystem/         Stat pipeline, Level up
│   ├── QuestSystem/        Quest data SO, Tracker, Dialogue
│   ├── Economy/            Ash/Spirit Ember wallet, Merchant, Blacksmith
│   └── Farming/            FarmPlot, Plant timer, Harvest
│
└── UI/
    ├── HUD/                HP/MP/Stamina, Skill slots, EXP bar
    ├── Menus/              Main menu, Character select, Pause
    ├── GraceMenu/          Level up, Teleport, Inventory, Map
    └── Shared/             BaseUi, DOTween helpers
```

---

## 🔗 Dependency Tree

```
├── [CORE]
│   ├── EventHub.cs (SO)        Pub/sub toàn bộ game — không direct reference
│   ├── ServiceLocator.cs       DI container
│   └── StateMachine<T>         Generic FSM
│
├── [GAME STATE]
│   └── GameManager.cs          FSM: MainMenu → CharSelect → Playing → Paused → Dead
│
├── [PLAYER]
│   └── PlayerController.cs
│         ├── PlayerCombat.cs
│         ├── PlayerStats.cs
│         ├── PlayerSkills.cs
│         └── HealthSystem.cs
│
├── [ENEMY]
│   └── EnemyBase.cs            FSM: Patrol → Detect → Chase → Attack → Dead
│         ├── BossBase.cs
│         └── EnemySpawner.cs
│
├── [GRACE POINT]
│   └── GracePoint.cs           Activate → heal → save → respawn anchor
│
├── [ECONOMY]
│   ├── WalletService.cs        [IWalletService]
│   ├── MerchantSystem.cs
│   └── BlacksmithSystem.cs
│
└── [UI]
    └── UIManager.cs
          └── BaseUi.cs
```

---

## 🏗️ Kiến trúc — 4 Pattern chính

### 1. EventHub — SO-based Pub/Sub

```csharp
// Subscribe:
eventHub.playerEvents.onPlayerDead.Subscribe(OnPlayerDead);

// Publish:
eventHub.playerEvents.onPlayerDead.Raise();

// Unsubscribe (OnDisable bắt buộc):
eventHub.playerEvents.onPlayerDead.Unsubscribe(OnPlayerDead);
```

### 2. Service Locator — Dependency Injection

```csharp
// Register (Awake):
ServiceLocator.Register<IWalletService>(this);

// Consume (anywhere):
int ash = ServiceLocator.Get<IWalletService>()?.Ash ?? 0;

// Unregister (OnDestroy):
ServiceLocator.Unregister<IWalletService>();
```

**Services dự kiến:**

| Interface | Implementation |
| --- | --- |
| `IWalletService` | `WalletService` |
| `IInventoryService` | `InventoryManager` |
| `IStatService` | `PlayerStats` |
| `IQuestService` | `QuestTracker` |
| `ISaveService` | `SaveSystem` |
| `IUIService` | `UIManager` |

### 3. Generic StateMachine

```csharp
var fsm = new StateMachine<PlayerState>(states, PlayerState.Idle);
fsm.Initialize();
fsm.ChangeState(PlayerState.Attack);
// Tick trong Update/FixedUpdate
fsm.Tick();
fsm.FixedTick();
```

### 4. Object Pooler

```csharp
var projectile = projectilePool.GetFromPool(spawnPos, rotation);
// Tự trả về pool qua IPoolableWithInit<T>
```

---

## ⚙️ Setup

1. Clone repo và mở bằng Unity Hub
2. Mở scene chính: `Assets/Ashfall/Scenes/Main.unity`
3. Build: `File → Build Settings` chọn platform

---

## 📝 Coding Conventions

- **Events:** Dùng `eventHub.*` — không dùng C# event trực tiếp
- **Services:** `ServiceLocator.Get<T>()` — luôn null-check (`?.`)
- **StateMachine:** Mỗi state là 1 class riêng implement `IState`
- **EventHub:** Luôn `Unsubscribe` trong `OnDisable`
- **TimeScale:** Chỉ `GameManager` quản lý `Time.timeScale`
- **Stats:** base + stat point + equipment + passive = final stat
- **Pooler:** Mọi object spawn nhiều lần đều dùng pool
- **SO:** ItemData, SkillData, QuestData, EnemyData đều là ScriptableObject
- **Prefix:** Tất cả script dùng prefix `AF_`

---

## 🌿 Branch Convention

| Branch | Mục đích |
| --- | --- |
| `main` | Production — stable |
| `develop` | Integration branch |
| `feature/<tên>` | Tính năng mới |
| `fix/<tên>` | Bug fix |
| `claude/<tên>` | AI-assisted development |

---

*Unity 2022 · C# · PC & Mobile · DOTween · Cinemachine · TextMesh Pro*
