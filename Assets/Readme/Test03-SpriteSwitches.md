# Test03: Sprites with physics components (the two switches)

**Scene:** `Assets/Scenes/Test03.unity`
**Script:** `Assets/Scripts/Test03SpriteSwitches.cs` (on the `Test03SpriteSwitches` GameObject)

## What it shows

1. **The component workflow.** Every object in this scene is a normal GameObject with:
   - a **SpriteRenderer** (Unity's built-in Circle or Square sprite),
   - a **Physics Pose** (the body: Static for the ground and ramp, Dynamic for the ball and boxes),
   - a **Physics Area Sprite** (the shape, taken from the sprite's physics outline).

   No code creates these objects. The script only adds keyboard controls.

2. **The two transform write switches.** A sprite follows its physics body only when **both** are on:

| Switch | Values | Where to set it |
|---|---|---|
| **World** Transform Write Mode | Off / **Fast 2D** / Slow 3D / Custom | *Project Settings > Physics Core 2D*, or a Simulation Definition asset |
| **Pose** Transform Write Mode | Current / **Interpolate** / Extrapolate / Off | *Physics Pose > Definition* |

## How to run

Open the scene, press **Play** and click the **Game** view so it has keyboard focus.

| Key | Action |
|---|---|
| **W** | Toggle the **world** switch (Off / Fast 2D) |
| **P** | Toggle the **Pose** switch of every sprite (Off / Interpolate) |
| **R** | Drop the sprites again from their start positions |

The top-left corner shows the state of both switches.

## What to look for

- With both switches on, the sprites fall, roll down the ramp and land.
- Press **W** (or **P**), then **R**: the **sprites stay frozen**, but the **physics outlines** keep falling. The simulation still runs; it just stops writing to the Transforms.

## Key code

```csharp
// Switch 1: the world.
var world = PhysicsWorld.defaultWorld;
world.transformWriteMode = PhysicsWorld.TransformWriteMode.Fast2D;   // or Off

// Switch 2: each body. A Physics Pose component gives you its body.
var body = pose.body;
body.transformWriteMode = PhysicsBody.TransformWriteMode.Interpolate; // or Off
```

## Good to know

- In Unity 6.7, a **new** Physics Core Settings 2D asset turns the world switch **on** by default. The Sandbox and Workshop example projects have it **off**, because they only draw with the debug renderer.
- Writing Transforms costs time. Enable it only for bodies whose Transform something reads (sprites, cameras). Keep each moving body as the **root** of its own hierarchy so Unity can write the Transforms in parallel.
- If an Area Sprite has fewer shapes than expected, check **Rejected Shape Count** in its Info section. Points that lie on a straight line or are too close together are rejected.

## Presentation slides

10 (Physics Pose settings), 25 (gotchas), 26 (the two switches), 28 (Transform write cost).
