# 2D Physics Core test scenes

Small, readable test scenes for the Unity 6.7 **2D Physics Core** (`Unity.U2D.Physics`, built on Box2D v3).
Each test has one scene in `Assets/Scenes/`, one script in `Assets/Scripts/`, and a README in this folder.

| Test | What it shows | How it's built | Controls | README |
|---|---|---|---|---|
| **Test01** | Bodies, shapes, debug drawing | Code | none | [Test01-Basic.md](Test01-Basic.md) |
| **Test02** | Trigger, contact and hit events, layers | Code | none (watch the Console) | [Test02-TriggersAndContacts.md](Test02-TriggersAndContacts.md) |
| **Test03** | Sprites with Physics Pose + Area Sprite, and the two transform write switches | Components | W, P, R | [Test03-SpriteSwitches.md](Test03-SpriteSwitches.md) |
| **Test04** | Hinge, distance, slider and wheel joints, plus a breakable bridge | Code | Space | [Test04-Joints.md](Test04-Joints.md) |
| **Test05** | Ray cast and overlap point queries | Code | Mouse, left click | [Test05-Queries.md](Test05-Queries.md) |
| **Test06** | Performance: thousands of bodies with the batch API, physics on worker threads | Code | Space, C | [Test06-Performance.md](Test06-Performance.md) |
| **Test07** | Save and rewind the whole world (snapshots), with a determinism check | Code | R, S | [Test07-Snapshot.md](Test07-Snapshot.md) |
| **Test08** | Character controller with the Cast Mover | Code | Arrows or A/D, Space | [Test08-CastMover.md](Test08-CastMover.md) |
| **Test09** | Callbacks: contact, trigger and joint threshold in code, plus a UnityEvent on a Physics Area | Code + Components | Space | [Test09-Callbacks.md](Test09-Callbacks.md) |

## Running a test

1. Open the scene from `Assets/Scenes/`.
2. Press **Play**.
3. Click the **Game** view so it gets keyboard and mouse input. Unity only advances Play mode while the Editor is the focused window.

## Info panel (every scene)

Each scene has a **SampleInfo** GameObject that shows a UI Toolkit panel in the top-right corner:

- the sample's title and a short description,
- the list of **2D Physics Core APIs and components the sample uses**,
- live numbers from the physics world: bodies, shapes, joints, contacts and physics step time (ms).

Press **I** to hide or show it. To change the text, select **SampleInfo** in the scene and edit **Title**, **Description** and **Api Used** in the `SampleInfoPanel` component.

| File | What it is |
|---|---|
| `Assets/UI/SampleInfo/SampleInfoPanel.uxml` | The panel layout |
| `Assets/UI/SampleInfo/SampleInfoPanel.uss` | The panel style |
| `Assets/UI/SampleInfo/SampleInfoPanel.cs` | Fills in the text and the live numbers |
| `Assets/UI/SampleInfo/SampleInfoTheme.tss` | The UI theme |
| `Assets/UI/PanelSettings.asset` | Panel Settings (scales with screen size, 1920×1080 reference) |

## Project notes

- **Unity 6000.7.0b4**, URP 2D, **2D Physics Core package 1.1.0**, Input System only.
- Physics objects created in code are drawn by the **physics debug renderer**. The scripts set `world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll`, because this project's default (*Default Selected*) only draws selected objects, and code-created bodies can never be selected.
- The **Pipeline** package (`com.unity.pipeline`) lets the Unity CLI drive the Editor. It's pinned to **0.7.0-exp.1**, because 0.8.0-exp.1 doesn't compile with URP 17.6 (`CliCommandAttribute` was moved to a new assembly).
- `MyTest.cs` in `Assets/` is the first experiment, used by `SampleScene`.

## Learn more

- Manual: https://docs.unity3d.com/6000.7/Documentation/Manual/2d-physics-api/2d-physics-api-landing.html
- Official examples (Sandbox and Workshop): https://github.com/Unity-Technologies/PhysicsExamples2D
