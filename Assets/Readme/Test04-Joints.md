# Test04: Joints

**Scene:** `Assets/Scenes/Test04.unity`
**Script:** `Assets/Scripts/Test04Joints.cs` (on the `Test04Joints` GameObject)

## What it shows

| Object | Joint | What it does |
|---|---|---|
| Orange arm (left) | **Hinge** | A pendulum swinging around a pin |
| Blue box | **Distance** + spring | Bounces up and down under a fixed point |
| Green box | **Slider** + motor + limits | Moves left and right along one axis, reversing every 3 seconds |
| Purple car | **Wheel** (×2) | Two wheels with spring suspension. The rear wheel has a motor and reverses every 3 seconds |
| Wooden bridge (right) | **Hinge** chain | Planks joined end to end and pinned at both sides. It **breaks** when overloaded |

## How to run

Open the scene, press **Play** and click the **Game** view.

| Key | Action |
|---|---|
| **Space** | Drop a heavy red box onto the bridge |

When the bridge breaks, the Console shows which joint snapped, for example:

```
Bridge joint 8 broke at 2687 N.
```

## Key code

**Creating a joint.** Fill in a definition with the two bodies and where each one is attached:

```csharp
world.CreateJoint(new PhysicsHingeJointDefinition
{
    bodyA = ground,
    bodyB = arm,
    localAnchorA = ground.GetLocalPoint(pin),   // the pin, in body A's local space
    localAnchorB = arm.GetLocalPoint(pin)       // the same pin, in body B's local space
});
```

**Breaking a joint.** There's no "break force" setting. Read the force and decide yourself:

```csharp
var force = joint.currentConstraintForce.magnitude;   // newtons
if (force > bridgeBreakForce)
    joint.Destroy();
```

**Changing a running joint:**

```csharp
m_Slider.motorSpeed = -m_Slider.motorSpeed;   // reverse the motor
```

## Things to try

- Change `bridgeBreakForce` in the Inspector (default 2000 N). At rest, each bridge joint carries about 60 N.
- Set `enableSpring = false` on the distance joint to turn it into a rigid rod.
- Set `enableLimit = true` with `lowerAngleLimit` / `upperAngleLimit` on the pendulum hinge.

## Good to know

- Classic Unity joints break on their own (`breakForce`, `OnJointBreak2D`). Physics Core gives you the force (`currentConstraintForce`), or a **force/torque threshold** that sends an `OnJointThreshold2D` callback, and **you** decide what happens.
- Most joints have **springs**, **motors** and **limits**.
- `collideConnected` is **off** by default: two connected bodies don't collide with each other.
- Physics Core has 7 joint types: Distance, Fixed, Hinge, Slider, Wheel, Relative and Ignore. There's no Spring joint (use Distance with a spring), no Friction joint, and no Target joint (use Relative).

## Presentation slides

11 (constraints / joints), 3.2 (Box2D v2 vs v3 joints).
