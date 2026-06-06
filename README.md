# Ashfall: Puzzle Chronicle

> A Mid-core Match-3 RPG Hybrid — built in Unity 6.4 URP

---

## Overview

Ashfall: Puzzle Chronicle translates high-intensity RPG mechanics into a Turn-based, Path-Matching framework. Pivoting from its original 2.5D Action-RPG roots, this project is a vertical slice demo focused on demonstrating clean architecture, modular UI systems, and market-ready gameplay.

**Target scope:** 3 playable levels · 1 Boss · Core meta-game (Loadout, Shop, Additive 3D Map)

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 6.4 (URP) |
| Language | C# |
| Platform | PC (Windows) |
| UI Architecture | Multi-Scene Additive Loading (UI Overlay + 3D Diorama) |

**Key packages:** Unity Input System, Cinemachine, Odin Inspector, DOTween, TextMesh Pro.

---

## Gameplay Architecture

### Split-Screen Blueprint
- **Top 1/3 (Action Field):** 2.5D stationary arena utilizing legacy Z-locked physics. Player and Enemy units face each other, triggering combat states based on grid events.
- **Bottom 2/3 (Puzzle Grid):** 2D Grid Array for Path-Matching. Players connect adjacent tokens (orthogonally and diagonally) to execute actions.

### Core Combat (Turn-Based)
Real-time stamina and posture systems have been refactored out in favor of a strictly turn-based loop:
- **Sword Token:** Triggers Player `Attack` state. Damage scales based on Base ATK and the length of the connected chain.
- **Shield Token:** Triggers `Block` state. Grants temporary Armor/Barrier points to absorb incoming enemy damage during the Enemy Turn.
- **Orb Token:** Grants Mana to charge the player's active Normal Skills.

### Skill System (Modular Data)
Skills are data-driven using `ScriptableObject` (`SkillData`) for easy expansion and loadout swapping.
- **Normal Skills:** Consume Mana (charged via Orb tokens).
- **Ultimate Skills:** Charged strictly via dealing damage or defeating enemies.

---

## Progression & UI (Meta-Game)

A unified Hub Menu using a 4-tab Bottom Navigation system:
- **Map (Additive 3D Scene):** Loaded asynchronously behind the UI Canvas. Uses legacy 3D environmental assets to create a static Node Map diorama. Progression unlocks consecutive nodes.
- **Equipment & Skills:** Loadout management interfaces to equip weapons and assign active skills.
- **Shop:** Upgrade loops (e.g., enhancing Base ATK) using in-game currency earned from clearing levels.

---

## Architecture & Coding Conventions

### Reused Legacy Systems
- **State Machine (FSM):** Streamlined to core necessary states: `Idle`, `Attack`, `Block`, `Skill`, `TakeHit`, `Dead`.
- **HealthSystem:** Event-driven HP management, now supporting Armor/Barrier layers.

### Key Design Patterns
- **Data / Visual Separation:** `SkillData` and `LevelData` ScriptableObjects dictate logic independently of visual prefabs.
- **Event-Driven Combat:** Grid actions broadcast events (e.g., `OnPathMatched`) listened to by the FSM to trigger animations and apply damage.
- **Additive Scene Management:** Main UI exists in a persistent Scene. The 3D Map diorama is loaded/unloaded asynchronously to preserve memory and ensure `EventSystem` singularity.

---

## Coding Conventions

| Convention | Rule |
|---|---|
| Namespace | `_Ashfall._Scripts.*` |
| Language | English only in source files |
| ScriptableObjects | Used for all immutable data (`SkillData`, `LevelData`, `ItemData`) |
| Additive Scenes | `EventSystem` and `AudioListener` strictly isolated to the Main UI Scene |