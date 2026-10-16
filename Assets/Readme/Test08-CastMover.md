# Test08: Character controller with the Cast Mover

**Scene:** `Assets/Scenes/Test08.unity`
**Script:** `Assets/Scripts/Test08CastMover.cs` (on the `Test08CastMover` GameObject)

## What it shows

A platformer character that is **not a physics body**. It's just a capsule shape that the script moves. Every physics step:

1. **Ground check:** a small circle is cast a few centimetres down from the feet.
2. **Velocity:** walk left or right, jump if on the ground, otherwise fall with gravity.
3. **Cast Mover:** `world.CastMover` sweeps the capsule towards the target position, stops it at walls, slides it along slopes, and returns where it ended up and the corrected velocity.

The level has a slope, two platforms, a wall and three loose boxes. The capsule is **orange** on the ground and **blue** in the air.

## How to run

Open the scene, press **Play** and click the **Game** view.

| Key | Action |
|---|---|
| **Left / Right** or **A / D** | Walk |
| **Space** | Jump |

## Key code

```csharp
var input = new PhysicsQuery.WorldMoverInput
{
    geometry = capsule,                                  // the character's shape
    transform = currentTransform,                        // where it is now
    targetPosition = position + velocity * deltaTime,    // where it wants to go
    velocity = velocity,
    maxIterations = 5,
    moveTolerance = 0.01f,
    overlapFilter = PhysicsQuery.QueryFilter.Everything,
    castFilter = PhysicsQuery.QueryFilter.Everything
};

var result = world.CastMover(input);
currentTransform = result.transform;   // where it ended up
velocity = result.velocity;            // velocity after sliding along surfaces
```

The movement runs in `PhysicsEvents.PreSimulate`, so it happens once per physics step, with the step's `deltaTime`.

## Why the ground check matters (three bugs found while testing)

- **Sliding down slopes:** if gravity is always applied, the mover pushes the capsule into the slope every step and slides it down, even when standing still. Fix: when on the ground, set the vertical velocity to 0.
- **Jumps cancelled:** right after a jump starts, the feet are still within the ground-check distance, so the character looked "grounded" and the jump was reset. Fix: ignore the ground for a short moment (0.2 s) after jumping.
- **No jumping on the ramp:** a first fix ignored the ground whenever the character moved up. But walking up a ramp also moves you up, because the mover slides you along the slope, so you were never "grounded" there. The jump timer above fixes both cases.

The official *CharacterMover* example (Sandbox and Workshop) solves both with a "pogo": a spring-like ray under the character. It's more advanced and smoother on steps.

## Good to know

- Why not a dynamic body? Dynamic characters get pushed around, bounce and slide. Kinematic bodies pass through walls. The Cast Mover gives exact, game-like control, with collisions solved by the engine.
- The character has **no shape in the world**, so other bodies don't collide with it, and the debug renderer doesn't draw it. The script draws it with `world.DrawGeometry`.
- Each Physics Area has **Mover Data** (`pushLimit`, `clipVelocity`) to control how that surface affects movers.
- The Cast Mover was introduced in Box2D 3.1 (where it was marked experimental).

## Presentation slides

14 (queries and the Cast Mover), 3.2 (new in Box2D v3).
