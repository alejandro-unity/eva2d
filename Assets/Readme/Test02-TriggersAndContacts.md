# Test02: Triggers and collision detection

**Scene:** `Assets/Scenes/Test02.unity`
**Script:** `Assets/Scripts/Test02TriggersAndContacts.cs` (on the `Test02TriggersAndContacts` GameObject)

## What it shows

The three event types of 2D Physics Core:

| Event | What happens on screen |
|---|---|
| **Trigger** begin / end | A ball turns **green** while it is inside the yellow zone, and gray again when it leaves |
| **Contact** begin / end | A ball turns **red** while it touches the ground |
| **Hit** | A hard impact on the ground is written to the Console, with its speed and position |

A ball spawns every 0.75 seconds. When there are 25, the oldest is removed.

## How to run

Open the scene, press **Play**, and keep the **Console** window open to see the hit messages, for example:

```
Hit: a ball struck the ground at 7.0 m/s at (-0.90, 0.00).
```

## Key code

**1. Each shape opts in to the events it wants.** Both shapes in a pair must enable an event type:

```csharp
var triggerDef = new PhysicsShapeDefinition
{
    isTrigger = true,      // overlaps without colliding
    triggerEvents = true,
    ...
};

var ballDef = new PhysicsShapeDefinition
{
    triggerEvents = true,
    contactEvents = true,
    hitEvents = true,
    ...
};
```

**2. Layers say what each shape is and what it touches** (64-bit masks):

```csharp
contactFilter = new PhysicsShape.ContactFilter
{
    categories = new PhysicsMask { bitMask = BallLayer },                                // what I am
    contacts   = new PhysicsMask { bitMask = GroundLayer | TriggerLayer | BallLayer }    // what I touch
}
```

**3. Events are read after each simulation step:**

```csharp
PhysicsEvents.PostSimulate += OnPostSimulate;

void OnPostSimulate(PhysicsWorld world, float timeStep)
{
    foreach (var e in world.triggerBeginEvents) { /* e.triggerShape, e.visitorShape */ }
    foreach (var e in world.contactBeginEvents) { /* e.shapeA, e.shapeB */ }
    foreach (var e in world.contactHitEvents)   { /* e.approachSpeed, e.point */ }
}
```

## Good to know

- Events are delivered as **lists after the step**, not as callbacks during it. That's what allows multithreading and deterministic results.
- **Always check `isValid`**: a shape may already be destroyed by the time you read its event.
- **Contact** and **hit** events need a dynamic or kinematic body. **Hit** events only fire above the world's *Contact Hit Event Threshold* (1 m/s by default).
- Triggers don't detect other triggers, and they don't use continuous collision detection.

## Presentation slides

12 (filtering and layers), 13 (events and callbacks).
