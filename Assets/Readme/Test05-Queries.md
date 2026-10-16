# Test05: Queries (asking the world questions)

**Scene:** `Assets/Scenes/Test05.unity`
**Script:** `Assets/Scripts/Test05Queries.cs` (on the `Test05Queries` GameObject)

## What it shows

Queries **read** the world without changing it. This test uses two kinds:

| Query | What happens on screen |
|---|---|
| **Ray cast** | A line from the white dot (top left) to the mouse. **Green** = it hit a shape, with the hit point and a **yellow** surface normal. **Red** = nothing hit |
| **Overlap point** | The shape under the mouse turns **yellow** |

**Clicking** uses a query result to act: it pushes the shape under the mouse upwards.

## How to run

Open the scene, press **Play**, and move the mouse over the **Game** view.

| Input | Action |
|---|---|
| Move the mouse | Aim the ray and highlight the shape under the cursor |
| **Left click** | Push the shape under the mouse upwards |

## Key code

**Ray cast.** A line segment from an origin, moving by a translation:

```csharp
var ray = new PhysicsQuery.CastRayInput
{
    origin = rayOrigin,
    translation = mousePosition - rayOrigin,
    maxFraction = 1f
};

using var hits = world.CastRay(ray, PhysicsQuery.QueryFilter.Everything, PhysicsQuery.WorldCastMode.Closest);
if (hits.Length > 0)
{
    var hit = hits[0];   // hit.point, hit.normal, hit.shape, hit.fraction
}
```

**Overlap point.** Which shapes contain this point?

```csharp
using var overlaps = world.OverlapPoint(mousePosition, PhysicsQuery.QueryFilter.Everything);
if (overlaps.Length > 0)
{
    var shape = overlaps[0].shape;
    shape.body.ApplyLinearImpulseToCenter(new Vector2(0f, 8f), true);
}
```

## Good to know

- Query results are **native arrays**. Always dispose them; `using var` does that for you.
- `WorldCastMode.Closest` returns only the first hit. `AllSorted` returns every hit, ordered by distance.
- Use a `QueryFilter` (`categories` / `hitCategories`) to limit which layers a query can hit.
- Queries are **read operations**, so many can run in parallel inside **jobs**, using the same API. The Sandbox *Queries* benchmark casts thousands of rays per frame this way.
- Other queries to explore: `CastShape` / `CastShapeProxy` (sweep a shape), `TestOverlapPoint` (yes/no answer), and `CastMover` (character controller).

## Presentation slides

14 (queries), 18 and 28 (jobs and threads).
