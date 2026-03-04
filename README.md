# Shadow Extraction

A 3D tactical extraction shooter prototype built in Unity 6 (URP) for CMP-6056B / CMP-7042B coursework at the University of East Anglia.

---

## 🎮 Game Overview

Shadow Extraction is inspired by Escape from Tarkov. The player is inserted into a hostile open-world compound, must locate loot, interact with the environment, and reach the extraction point to win. The camera uses a god-view overhead perspective (offset: 0, 15, −10 / pitch: 60°), and the mouse cursor controls the character's 360-degree facing direction independently of movement — keyboard moves the body, mouse aims the weapon.

---

## ✅ Prototype Status

| Feature | Script | Status |
|---|---|---|
| WASD movement (CharacterController, gravity applied) | `PlayerMovement.cs` | ✅ Working |
| God-view camera (LateUpdate follow, offset 0/15/−10) | `CameraFollow.cs` | ✅ Working |
| Mouse-driven orientation (Plane.Raycast + Quaternion.Slerp) | `PlayerMovement.cs` | ✅ Working |
| Rigidbody bullet shooting (left click, FirePoint, bulletSpeed 40) | `PlayerAimAndShoot.cs` | ✅ Working |
| Player health system (TakeDamage / Heal / HealthBar Slider UI) | `PlayerHealth.cs` | ✅ Working |
| Player stats tracking (health, ammo) | `PlayerStats.cs` | ✅ Working |
| IInteractable interface | `IInteractable.cs` | ✅ Working |
| Trigger-based interaction (OnTriggerEnter/Exit + E key + TMP prompt) | `PlayerInteraction.cs` | ✅ Working |
| Loot crate (open / already looted state) | `LootCrate.cs` | ✅ Working |
| Chest UI open/close (InventoryPanel toggle) | `ChestLoot.cs` | ✅ Working |
| Item pickup (name + amount, Destroy on pickup) | `PickupItem.cs` | ✅ Working |
| 8×8 inventory grid UI (hidden by default) | `InventoryUI.cs` | ✅ Working |
| Enemy AI (NavMesh patrol / chase / attack) | — | 🔄 Planned |
| Extraction zone win condition | — | 🔄 Planned |

---

## 🕹️ Controls

| Input | Action |
|---|---|
| W / A / S / D | Move character (world-space axes) |
| Mouse movement | Rotate character to face cursor position |
| Left Mouse Button | Fire weapon |
| E | Interact with object in trigger range |
| K | Take 10 damage (debug test key) |
| H | Heal 10 HP (debug test key) |

---

## 📁 Project Structure

```
Assets/
├── Scenes/
│   └── MainLevel                    # Main game scene
├── Scripts/
│   ├── Player/
│   │   ├── PlayerMovement.cs        # WASD movement + mouse orientation (Plane.Raycast + Slerp)
│   │   ├── PlayerAimAndShoot.cs     # Ground raycast aiming + Rigidbody bullet firing
│   │   ├── PlayerHealth.cs          # Health system + HealthBar Slider UI
│   │   ├── PlayerStats.cs           # Health and ammo stat tracking
│   │   └── CameraFollow.cs          # God-view camera rig (LateUpdate, offset 0/15/-10)
│   └── Interaction/
│       ├── IInteractable.cs         # Interaction interface (Interact + GetPromptText)
│       ├── PlayerInteraction.cs     # Trigger detection + E key + TextMeshPro prompt
│       ├── LootCrate.cs             # Loot crate (open state + isLooted flag)
│       ├── ChestLoot.cs             # Chest UI panel toggle (InventoryPanel)
│       ├── PickupItem.cs            # Generic item pickup (Destroy on interact)
│       └── InventoryUI.cs           # 8x8 inventory grid (hidden by default)
├── Prefabs/
│   └── BulletTemp                   # Rigidbody bullet prefab
Packages/                            # Unity package manifest (URP, TextMeshPro)
ProjectSettings/                     # Physics layers (Ground), input axes, tags
.gitignore                           # Standard Unity gitignore
```

---

## 🛠️ How to Open the Project

1. Install **Unity 6.3 LTS (6000.3.8f1)** via Unity Hub
2. Clone this repository:
   ```
   git clone https://github.com/Noonelav/Unity-3D-Game-Prototype.git
   ```
3. Open Unity Hub → **Add project from disk** → select the cloned folder
4. Open the scene: `Assets/Scenes/MainLevel`
5. Press **Play** to run

> ⚠️ Requires Unity 6 LTS. Opening in an earlier version may cause compatibility issues with URP shaders and the `rb.linearVelocity` API used in `PlayerAimAndShoot.cs`.

---

## 📹 Video Demo

[INSERT YOUTUBE LINK]

---

## 📋 Module Info

| | |
|---|---|
| Module | CMP-6056B / CMP-7042B |
| Assignment | Video Game Design / Plan 001 |
| Student | Shuhao Chai — 100526360 |
| University | University of East Anglia |
| Engine | Unity 6.3 LTS (6000.3.8f1) — URP |
