# Test07: Snapshot, rewind and determinism

**Scene:** `Assets/Scenes/Test07.unity`
**Script:** `Assets/Scripts/Test07Snapshot.cs` (on the `Test07Snapshot` GameObject)

## What it shows

1. **Snapshots:** the whole physics world (bodies, shapes, joints, contacts, everything) is saved into memory, and later **restored exactly**. This is the basis for rollback netcode, replays and "rewind time".
2. **Determinism:** after rewinding, the simulation replays **bit for bit** the same. To prove it, the script turns the exact position and rotation of every body into one number (a hash) 100 steps after the snapshot, and compares the replay with the first run.

A heavy ball is thrown at a pyramid of boxes. A snapshot is taken at the start.

## How to run

Open the scene, press **Play** and click the **Game** view.

| Key | Action |
|---|---|
| **R** | Rewind to the snapshot and watch the exact same crash again |
| **S** | Take a new snapshot now |

The top-left corner shows the steps since the snapshot and the result, for example:

```
Replay IDENTICAL to the first run (hash 3771E1FC833EB2C3).
```

Tested: rewound several times, and every replay gave the same hash.

## Key code

```csharp
// Save the world (dispose it when you no longer need it).
var snapshot = world.CreateSnapshot(Allocator.Persistent);
Debug.Log($"Snapshot size: {snapshot.Length} bytes");

// Rewind. The body handles you already hold stay valid.
world.ApplySnapshot(snapshot);

// Clean up.
snapshot.Dispose();
```

You can also create a **new world** from a snapshot with `PhysicsWorld.Create(snapshot, definition)`, for example to run a "what if" prediction without touching the main world.

## Good to know

- **`ApplySnapshot` doesn't restore world settings such as debug drawing.** After a rewind, `drawOptions` went back to the project default (*Default Selected*) and nothing was drawn. The script sets `world.drawOptions` again after every rewind.
- Rollback is only deterministic if **your own code** is too: same inputs, and random numbers from a fixed seed (`Unity.Mathematics.Random`).
- Box2D v3 is also deterministic **across platforms** (x64 / ARM) and **independent of the number of threads**.
- Snapshots cover the whole world. Large worlds make large snapshots, and you can't save only part of a world.

## Presentation slides

3.2 (Box2D v2 vs v3: determinism and rollback).
