# Test06: Performance (batch API and jobs)

**Scene:** `Assets/Scenes/Test06.unity`
**Script:** `Assets/Scripts/Test06Performance.cs` (on the `Test06Performance` GameObject)

## What it shows

Thousands of bodies handled with **one call each**, instead of one call per body:

| Step | Batch call |
|---|---|
| Create N bodies | `world.CreateBodyBatch(definition, count, allocator)` |
| Place them all | `PhysicsBody.SetBatchTransform(transforms)` |
| Remove them all | `PhysicsWorld.DestroyBodyBatch(bodies)` |

The simulation itself runs on **worker threads (jobs)**, up to 64 cores.

## How to run

Open the scene, press **Play** and click the **Game** view. It starts with 2,000 bodies falling into a container.

| Key | Action |
|---|---|
| **Space** | Add 1,000 more bodies |
| **C** | Remove all bodies |

The **info panel** (top right) shows the live body, shape and contact counts and the **physics step time** (ms). The script also has an `OnGUI` overlay with the frame time and worker count; it's commented out, so uncomment it if you want it.

### Seeing the jobs in the Profiler

1. Open **Window > Analysis > Profiler** and make sure the target dropdown says **Play Mode**.
2. In the **CPU Usage** module, switch the bottom panel from **Hierarchy** to **Timeline**. Hierarchy only shows the Main Thread.
3. Expand the **Job** group to see **Worker 0, Worker 1, …**
4. Type `PhysicsCore2D` in the search box. Collision detection is `PhysicsCore2D.Collide`, and solving is `Solve Contact` / `Relax Contact`, inside `EngineJob` blocks.
5. Click a frame that contains `FixedUpdate.PhysicsCore2DFixedUpdate`. Physics runs at the fixed timestep, so not every frame has a physics step.

On the **Main Thread** you'll only see `PhysicsCore2D.Step` scheduling the work, and `PhysicsWorld.FinishTask` waiting for the workers.

## Measured on this Mac (Editor, Unity 6000.7.0b4)

| Bodies | Contacts | Physics step |
|---|---|---|
| 2,000 | ~6,700 | ~1.6 ms |
| 5,000 | ~19,000 | ~1.0–1.5 ms |

Worker threads: 16 (the machine's core count).

> **The number to watch is "Physics step", not FPS.** In the Editor, the frame rate also includes drawing thousands of debug shapes, and it drops a lot when Unity isn't the focused window.

## Key code

```csharp
// 1. Create all the bodies in one call.
using var bodies = world.CreateBodyBatch(bodyDef, count, Allocator.Temp);

// 2. Set all their positions in one call.
var transforms = new NativeArray<PhysicsBody.BatchTransform>(count, Allocator.Temp);
for (var i = 0; i < count; i++)
    transforms[i] = new PhysicsBody.BatchTransform { physicsBody = bodies[i], position = ..., rotation = ... };
PhysicsBody.SetBatchTransform(transforms);
transforms.Dispose();

// 3. Remove them all in one call.
PhysicsWorld.DestroyBodyBatch(allBodies.AsArray());

// Live statistics.
var counters = world.counters;   // bodyCount, contactCount, ...
var profile  = world.profile;    // simulationStep (ms), solving, broadphaseUpdates, ...
```

## Good to know

- Batch calls avoid crossing from C# to the engine thousands of times. There are batch versions for velocities, forces and shape creation too.
- **Read** operations (queries, positions, velocities, events) can run in parallel in jobs. **Write** operations are best done on the main thread with these batch calls.
- `DestroyBodyBatch` is **static** (`PhysicsWorld.DestroyBodyBatch`), because each body already knows its world.
- Set the worker count per world with **Simulation Workers** in the world definition. Don't change it at runtime.
- No GameObjects are used. Thousands of GameObjects would cost far more than the physics does.

## Presentation slides

3 (why a new system), 18 (performance: batching, jobs and threads), 28 (jobs, threads and the WORM lock).
