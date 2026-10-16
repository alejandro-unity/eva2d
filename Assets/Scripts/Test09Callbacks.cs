using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test09: callbacks. Instead of reading event lists (Test02), the engine CALLS our methods.
///   1. Contact callback:  balls hit a round bumper. The bumper's callback target is this script,
///                         so OnContactBegin2D is called, and the bumper flashes.
///   2. Trigger callback:  balls fall through a zone. OnTriggerBegin2D / OnTriggerEnd2D colour them.
///   3. Joint threshold:   a box hangs on a rope (distance joint) with a force threshold.
///                         Press SPACE to drop a heavy ball on it. OnJointThreshold2D cuts the rope.
///   4. Component callback (right side, set up in the scene, no code): a Physics Area with
///                         Callback Source = Events calls a UnityEvent (Test09Flash.Flash).
/// </summary>
public class Test09Callbacks : MonoBehaviour,
    PhysicsCallbacks.IContactCallback,
    PhysicsCallbacks.ITriggerCallback,
    PhysicsCallbacks.IJointThresholdCallback
{
    [Tooltip("The component bumper on the right, to show its hit count.")]
    public Test09Flash componentBumper;

    public float ropeBreakForce = 150f;

    private PhysicsWorld m_World;
    private PhysicsShape m_Bumper;
    private PhysicsDistanceJoint m_Rope;
    private int m_BumperHits;
    private float m_BumperFlash;
    private string m_RopeStatus = "Rope: holding";

    private readonly Queue<PhysicsBody> m_Balls = new();
    private float m_SpawnTimer;
    private bool m_SpawnLeft = true;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Joint threshold callbacks are only sent automatically when the world allows it.
        // (Contact and trigger callbacks are always sent.)
        m_World.autoJointThresholdCallbacks = true;

        // Ground (no callbacks).
        m_World.CreateBody().CreateShape(new SegmentGeometry { point1 = new Vector2(-14f, 0f), point2 = new Vector2(14f, 0f) }, PhysicsShapeDefinition.defaultDefinition);

        CreateTriggerZone();
        CreateBumper();
        CreateHangingBox();
    }

    // 1. A static round bumper. Its callback target is this script.
    private void CreateBumper()
    {
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.contactEvents = true;
        shapeDef.surfaceMaterial.bounciness = 0.6f;
        shapeDef.surfaceMaterial.customColor = Color.orange;

        m_Bumper = m_World.CreateBody().CreateShape(new CircleGeometry { center = new Vector2(-7f, 2.5f), radius = 1.2f }, shapeDef);
        m_Bumper.callbackTarget = this;
    }

    // 2. A trigger zone above the bumper. Its callback target is this script too.
    private void CreateTriggerZone()
    {
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.isTrigger = true;
        shapeDef.triggerEvents = true;
        shapeDef.surfaceMaterial.customColor = Color.yellow;

        var zone = m_World.CreateBody().CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1.5f), radius: 0f, new PhysicsTransform(new Vector2(-7f, 6f))), shapeDef);
        zone.callbackTarget = this;
    }

    // 3. A box hanging on a rigid rope, with a force threshold on the joint.
    private void CreateHangingBox()
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = new Vector2(1f, 5f);
        var box = m_World.CreateBody(bodyDef);

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.surfaceMaterial.customColor = Color.cornflowerBlue;
        box.CreateShape(PolygonGeometry.CreateBox(new Vector2(2.5f, 0.4f), radius: 0f, PhysicsTransform.identity), shapeDef);

        var ground = m_World.CreateBody();
        m_Rope = m_World.CreateJoint(new PhysicsDistanceJointDefinition
        {
            bodyA = ground,
            bodyB = box,
            localAnchorA = new PhysicsTransform(new Vector2(1f, 9f)),
            localAnchorB = PhysicsTransform.identity,
            distance = 4f,
            forceThreshold = ropeBreakForce   // above this force, OnJointThreshold2D is called
        });
        m_Rope.callbackTarget = this;
    }

    // ---------- The callbacks: the engine calls these, on the main thread, after the simulation step ----------

    public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
    {
        // Only the bumper has this script as its target, so every call is a bumper hit.
        m_BumperHits++;
        m_BumperFlash = 0.15f;
    }

    public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent) { }

    public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent) => SetColor(beginEvent.visitorShape, Color.limeGreen);

    public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent) => SetColor(endEvent.visitorShape, Color.gray);

    public void OnJointThreshold2D(PhysicsEvents.JointThresholdEvent thresholdEvent)
    {
        var joint = thresholdEvent.joint;
        if (!joint.isValid)
            return;

        m_RopeStatus = $"Rope: CUT at {joint.currentConstraintForce.magnitude:F0} N";
        Debug.Log(m_RopeStatus);
        joint.Destroy();
    }

    // ---------- Spawning, input and drawing ----------

    private void Update()
    {
        // A ball every 0.5 s, alternating between the code bumper (left) and the component bumper (right).
        m_SpawnTimer += Time.deltaTime;
        if (m_SpawnTimer > 0.5f)
        {
            m_SpawnTimer = 0f;
            SpawnBall(new Vector2((m_SpawnLeft ? -7f : 7f) + Random.Range(-1.5f, 1.5f), 12f), 0.3f, density: 1f);
            m_SpawnLeft = !m_SpawnLeft;
        }

        // SPACE drops a heavy ball on the hanging box.
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            DropHeavyBall();

        // Flash the bumper white for a moment after each hit.
        m_BumperFlash -= Time.deltaTime;
        SetColor(m_Bumper, m_BumperFlash > 0f ? Color.white : Color.orange);
    }

    private void DropHeavyBall() => SpawnBall(new Vector2(1f, 12f), 0.5f, density: 20f);

    private void SpawnBall(Vector2 position, float radius, float density)
    {
        if (m_Balls.Count >= 40)
        {
            var oldest = m_Balls.Dequeue();
            if (oldest.isValid)
                oldest.Destroy();
        }

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = position;

        // Balls opt in to both event types, so the bumper and the zone can report them.
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.contactEvents = true;
        shapeDef.triggerEvents = true;
        shapeDef.density = density;
        shapeDef.surfaceMaterial.customColor = density > 1f ? Color.red : Color.gray;

        var ball = m_World.CreateBody(bodyDef);
        ball.CreateShape(new CircleGeometry { radius = radius }, shapeDef);
        m_Balls.Enqueue(ball);
    }

    private static void SetColor(PhysicsShape shape, Color color)
    {
        if (!shape.isValid)
            return;

        var surfaceMaterial = shape.surfaceMaterial;
        surfaceMaterial.customColor = color;
        shape.surfaceMaterial = surfaceMaterial;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 700, 25), $"Code bumper hits (OnContactBegin2D): {m_BumperHits}");
        GUI.Label(new Rect(10, 30, 700, 25), $"Component bumper hits (UnityEvent): {(componentBumper != null ? componentBumper.hitCount : 0)}");
        GUI.Label(new Rect(10, 50, 700, 25), $"{m_RopeStatus}    [SPACE] drop a heavy ball on the hanging box");
    }
}
