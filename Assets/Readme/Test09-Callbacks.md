# Test09: Callbacks

**Scene:** `Assets/Scenes/Test09.unity`
**Scripts:**
- `Assets/Scripts/Test09Callbacks.cs` (on the `Test09Callbacks` GameObject): the code callbacks.
- `Assets/Scripts/Test09Flash.cs` (on the `ComponentBumper` GameObject): a tiny method called by a UnityEvent.

## Events vs callbacks

| | **Events (Test02)** | **Callbacks (this test)** |
|---|---|---|
| Who does the work | **You** read lists from the world after each step | **The engine** calls methods on an object you choose |
| Code | `foreach (var e in world.contactBeginEvents) ...` | `OnContactBegin2D(ContactBeginEvent e) { ... }` |
| Good for | Processing many events at once, batching, jobs | Reacting per object, like `OnCollisionEnter2D` in classic Unity |

Both run **after** the simulation step, on the main thread.

## What it shows

| Object | Callback | What happens |
|---|---|---|
| Orange round **bumper** (left) | `IContactCallback.OnContactBegin2D` | Flashes white, and the hit counter goes up |
| Yellow **zone** above it | `ITriggerCallback.OnTriggerBegin2D / End2D` | Balls turn green inside the zone and gray when they leave |
| Blue **box on a rope** (middle) | `IJointThresholdCallback.OnJointThreshold2D` | When the rope's force goes over 150 N, the callback cuts it |
| Purple **component bumper** (right) | Physics Area, **Callback Source = Events**, **On Contact Begin** UnityEvent | Calls `Test09Flash.Flash()`, wired in the Inspector with no physics code |

Balls spawn every 0.5 s, alternating left and right.

## How to run

Open the scene, press **Play** and click the **Game** view.

| Key | Action |
|---|---|
| **Space** | Drop a heavy red ball on the hanging box. The rope is cut by the threshold callback |

The top-left corner shows both bumper hit counters and the rope status. The Console logs, for example:

```
Rope: CUT at 8349 N
```

## Key code

**1. Implement the interface, then set the object as the callback target:**

```csharp
public class Test09Callbacks : MonoBehaviour,
    PhysicsCallbacks.IContactCallback,
    PhysicsCallbacks.ITriggerCallback,
    PhysicsCallbacks.IJointThresholdCallback
{
    void Start()
    {
        bumperShapeDef.contactEvents = true;          // the shape must still opt in to events
        m_Bumper = body.CreateShape(circle, bumperShapeDef);
        m_Bumper.callbackTarget = this;               // send this shape's callbacks to me
    }

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent e) { /* e.shapeA, e.shapeB */ }
    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent e) { }
}
```

**2. Joint threshold callbacks need two extra steps:**

```csharp
world.autoJointThresholdCallbacks = true;   // off by default in this project's settings

m_Rope = world.CreateJoint(new PhysicsDistanceJointDefinition { ..., forceThreshold = 150f });
m_Rope.callbackTarget = this;

public void OnJointThreshold2D(PhysicsEvents.JointThresholdEvent e) => e.joint.Destroy();
```

**3. Component callback, no code.** On the Physics Area (here a Physics Area Sprite):
- **Definition > Contact Events** = on
- **Callbacks > Callback Source** = **Events**
- **Callbacks > Areas Only** = **off**, so contacts with shapes created in code (the balls) are reported too
- **Event settings > On Contact Begin**: drag the GameObject in and pick `Test09Flash > Flash()`

## Good to know

- **Contact and trigger callbacks are sent automatically.** Joint threshold (`autoJointThresholdCallbacks`) and body update (`autoBodyUpdateCallbacks`) callbacks are only automatic if the world allows it. Otherwise, call `world.SendJointThresholdCallbacks()` yourself (or `world.SendAllCallbacks()`).
- A shape still needs `contactEvents` / `triggerEvents` enabled. The callback target only decides **where** events are delivered.
- On a Physics Area, **Areas Only** is **on** by default. It only reports contacts where **both** shapes are Physics Areas. Turn it off to also hear about shapes created in code.
- Callback Source options on a Physics Area: **Off**, **Area** (a script implementing `PhysicsAreaCallbacks.IContactCallback`), **Shapes** (raw `PhysicsCallbacks`), or **Events** (UnityEvents).
- There are more callback types for advanced uses: `IContactFilterCallback` (decide if two shapes collide), `IPreContactCallback` (edit a contact before it's solved), `IBodyUpdateCallback`, and `ITransformWriteCallback`.
- During testing, the rope didn't break in the very first run, but it broke reliably in every run after that. If it doesn't break for you, check the Console and press Space again.

## Presentation slides

13 (events and callbacks), 11 (constraints: thresholds instead of break force).
