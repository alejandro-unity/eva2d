# eva2d: Unity 2D Physics Core examples

Small, readable test scenes for **Unity 2D Physics Core** (`Unity.U2D.Physics`), the new 2D physics engine in Unity 6.7, built on **Box2D v3**.

Each test is one scene, one short script, and one README explaining what it shows and the key code.

## Requirements

- **Unity 6000.7.0b4** (Unity 6.7 beta)
- Packages (already in `Packages/manifest.json`): `com.unity.2d.physics` 1.1.0, URP 2D, Input System

## Getting started

1. Clone the repo and open the folder in Unity 6000.7.0b4. Unity rebuilds `Library` on first open.
2. Open a scene from `Assets/Scenes/` and press **Play**.
3. Click the **Game** view so it receives keyboard and mouse input.
4. Every scene has an info panel (top right) listing the Physics Core APIs it uses, with live counters. Press **I** to hide or show it.

## Tests

| Scene | What it shows | Controls |
|---|---|---|
| **Test01** | Bodies, shapes and debug drawing, all in code | none |
| **Test02** | Trigger, contact and hit **events**, 64-bit layers | none (watch the Console) |
| **Test03** | Sprites with **Physics Pose + Physics Area Sprite** components, and the two transform write switches | W, P, R |
| **Test04** | Hinge, distance, slider and wheel **joints**, plus a breakable bridge | Space |
| **Test05** | **Queries**: ray cast and overlap point | Mouse, left click |
| **Test06** | **Performance**: thousands of bodies with the batch API, on worker threads | Space, C |
| **Test07** | **Snapshot and rewind** of the whole world, with a determinism check | R, S |
| **Test08** | Character controller with the **Cast Mover** | Arrows / A-D, Space |
| **Test09** | **Callbacks**: contact, trigger and joint threshold in code, plus UnityEvents on a Physics Area | Space |

Full details for each test are in [`Assets/Readme`](Assets/Readme/README.md).

## Project layout

| Folder | Contents |
|---|---|
| `Assets/Scenes` | Test01 to Test09 |
| `Assets/Scripts` | One script per test |
| `Assets/Readme` | One README per test, plus an index |
| `Assets/UI` | The UI Toolkit info panel and Panel Settings |

## Learn more

- [2D Physics Core manual (Unity 6.7)](https://docs.unity3d.com/6000.7/Documentation/Manual/2d-physics-api/2d-physics-api-landing.html)
- [Official examples: Sandbox and Workshop](https://github.com/Unity-Technologies/PhysicsExamples2D)
- [Box2D v3](https://box2d.org/)
