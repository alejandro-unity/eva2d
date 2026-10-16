# Test01: Basic physics in code

**Scene:** `Assets/Scenes/Test01.unity`
**Script:** `Assets/Scripts/Test01Basic.cs` (on the `Test01Basic` GameObject)

## What it shows

The smallest possible 2D Physics Core scene, built entirely in code:

- A **static** ground (a segment).
- A **dynamic** ball, a stack of 5 boxes and a capsule that lands off-centre and tips over.
- No GameObjects, sprites or renderers for the physics objects. The **physics debug renderer** draws every shape.

## How to run

Open the scene and press **Play**. There are no controls.

## What to look for

- The boxes stack and stay stable.
- The capsule hits the top of the stack, tips over and falls off.
- Each shape has its own colour, set with `surfaceMaterial.customColor`.

## Key code

```csharp
var world = PhysicsWorld.defaultWorld;

// Draw every shape, not only the selected ones.
world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

// Static ground: a body with no type set is Static.
var ground = world.CreateBody();
ground.CreateShape(new SegmentGeometry { point1 = new Vector2(-20, 0), point2 = new Vector2(20, 0) },
                   PhysicsShapeDefinition.defaultDefinition);

// Dynamic body: definition -> body -> shape.
var bodyDef = PhysicsBodyDefinition.defaultDefinition;
bodyDef.type = PhysicsBody.BodyType.Dynamic;
bodyDef.position = new Vector2(-3, 5);
world.CreateBody(bodyDef).CreateShape(new CircleGeometry { radius = 0.5f }, shapeDef);
```

## Good to know

- **Nothing visible?** Bodies created in code have no GameObject, so they can never be "selected". If the project's **Draw Options** are *Default Selected*, nothing is drawn. Setting `world.drawOptions = DefaultAll` fixes it. You can also set it in *Project Settings > Physics Core 2D > Settings*.
- Objects are created from **definitions**: plain structs you fill in, then pass to `CreateBody` / `CreateShape`.
- `PhysicsWorld`, `PhysicsBody` and `PhysicsShape` are lightweight **handles** (structs), not objects.

## Presentation slides

6 (object model), 8 ("Hello Physics" in code), 9 (shapes).
