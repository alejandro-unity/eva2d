using System.Collections.Generic;
using UnityEngine;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Test02: trigger and collision (contact) detection, built entirely in code.
/// Balls fall through a trigger zone and land on the ground.
/// - Trigger events: a ball turns green while it is inside the zone, and back to gray when it leaves.
/// - Contact events: a ball turns red when it touches the ground.
/// - Hit events: a hard impact on the ground is logged with its speed.
/// Events are read after each simulation step, using PhysicsEvents.PostSimulate.
/// </summary>
public class Test02TriggersAndContacts : MonoBehaviour
{
    // Collision layers (64-bit masks): what each shape is, and what it can touch.
    private const ulong GroundLayer = 1 << 0;
    private const ulong BallLayer = 1 << 1;
    private const ulong TriggerLayer = 1 << 2;

    private const float SpawnInterval = 0.75f;
    private const int MaxBalls = 25;

    private readonly Color m_BallColor = Color.gray;
    private readonly Color m_InsideTriggerColor = Color.limeGreen;
    private readonly Color m_TouchingGroundColor = Color.red;

    private readonly Queue<PhysicsBody> m_Balls = new();
    private Random m_Random = new(1234);
    private float m_SpawnTimer;

    private void OnEnable() => PhysicsEvents.PostSimulate += OnPostSimulate;
    private void OnDisable() => PhysicsEvents.PostSimulate -= OnPostSimulate;

    private void Start()
    {
        var world = PhysicsWorld.defaultWorld;
        world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Ground: reports contact and hit events against balls.
        // Both shapes in a pair must enable an event type for it to be reported.
        var groundDef = new PhysicsShapeDefinition
        {
            contactEvents = true,
            hitEvents = true,
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = GroundLayer },
                contacts = new PhysicsMask { bitMask = BallLayer }
            },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.6f, customColor = Color.white }
        };
        var ground = world.CreateBody();
        ground.CreateShape(PolygonGeometry.CreateBox(new Vector2(16f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, -0.5f))), groundDef);

        // Trigger zone: balls pass through it, and it reports when they enter and leave.
        var triggerDef = new PhysicsShapeDefinition
        {
            isTrigger = true,
            triggerEvents = true,
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = TriggerLayer },
                contacts = new PhysicsMask { bitMask = BallLayer }
            },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { customColor = Color.yellow }
        };
        var triggerZone = world.CreateBody();
        triggerZone.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 1.5f), radius: 0f, new PhysicsTransform(new Vector2(0f, 3f))), triggerDef);
    }

    private void Update()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_SpawnTimer < SpawnInterval)
            return;

        m_SpawnTimer = 0f;
        SpawnBall();
    }

    private void SpawnBall()
    {
        var world = PhysicsWorld.defaultWorld;

        // Keep the scene small: remove the oldest ball.
        if (m_Balls.Count >= MaxBalls)
        {
            var oldest = m_Balls.Dequeue();
            if (oldest.isValid)
                oldest.Destroy();
        }

        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = new Vector2(m_Random.NextFloat(-4f, 4f), 8f);

        // A ball enables all three event types, and can touch the ground, the trigger and other balls.
        var ballDef = new PhysicsShapeDefinition
        {
            triggerEvents = true,
            contactEvents = true,
            hitEvents = true,
            contactFilter = new PhysicsShape.ContactFilter
            {
                categories = new PhysicsMask { bitMask = BallLayer },
                contacts = new PhysicsMask { bitMask = GroundLayer | TriggerLayer | BallLayer }
            },
            surfaceMaterial = new PhysicsShape.SurfaceMaterial { friction = 0.6f, bounciness = 0.3f, customColor = m_BallColor }
        };

        var ball = world.CreateBody(bodyDef);
        ball.CreateShape(new CircleGeometry { radius = m_Random.NextFloat(0.25f, 0.5f) }, ballDef);
        m_Balls.Enqueue(ball);
    }

    // Called after every simulation step, so all events for that step are ready to read.
    private void OnPostSimulate(PhysicsWorld world, float timeStep)
    {
        // Trigger events: a ball entered or left the zone.
        foreach (var beginEvent in world.triggerBeginEvents)
            SetColor(beginEvent.visitorShape, m_InsideTriggerColor);

        foreach (var endEvent in world.triggerEndEvents)
            SetColor(endEvent.visitorShape, m_BallColor);

        // Contact events: a ball started or stopped touching the ground.
        foreach (var beginEvent in world.contactBeginEvents)
        {
            if (TryGetBallOnGround(beginEvent.shapeA, beginEvent.shapeB, out var ball))
                SetColor(ball, m_TouchingGroundColor);
        }

        foreach (var endEvent in world.contactEndEvents)
        {
            if (TryGetBallOnGround(endEvent.shapeA, endEvent.shapeB, out var ball))
                SetColor(ball, m_BallColor);
        }

        // Hit events: impacts faster than the world's Contact Hit Event Threshold (1 m/s by default).
        foreach (var hitEvent in world.contactHitEvents)
        {
            if (TryGetBallOnGround(hitEvent.shapeA, hitEvent.shapeB, out _))
                Debug.Log($"Hit: a ball struck the ground at {hitEvent.approachSpeed:F1} m/s at {hitEvent.point}.");
        }
    }

    // Returns the ball when a contact pair is a ball and the ground, in either order.
    private static bool TryGetBallOnGround(PhysicsShape shapeA, PhysicsShape shapeB, out PhysicsShape ball)
    {
        ball = default;

        // A shape can already be destroyed by the time its event is read.
        if (!shapeA.isValid || !shapeB.isValid)
            return false;

        if (IsOnLayer(shapeA, BallLayer) && IsOnLayer(shapeB, GroundLayer))
        {
            ball = shapeA;
            return true;
        }

        if (IsOnLayer(shapeB, BallLayer) && IsOnLayer(shapeA, GroundLayer))
        {
            ball = shapeB;
            return true;
        }

        return false;
    }

    private static bool IsOnLayer(PhysicsShape shape, ulong layer) => (shape.contactFilter.categories & layer) != 0;

    private static void SetColor(PhysicsShape shape, Color color)
    {
        if (!shape.isValid)
            return;

        var surfaceMaterial = shape.surfaceMaterial;
        surfaceMaterial.customColor = color;
        shape.surfaceMaterial = surfaceMaterial;
    }
}
