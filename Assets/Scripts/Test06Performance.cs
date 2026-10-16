using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Test06: performance, using the batch API.
/// Thousands of bodies are created with ONE call (CreateBodyBatch), placed with ONE call (SetBatchTransform)
/// and removed with ONE call (DestroyBodyBatch). The simulation itself runs on worker threads (jobs).
///   SPACE = add 1000 more bodies
///   C     = remove all bodies
/// Open Window > Analysis > Profiler to see the physics jobs running on the worker threads.
/// </summary>
public class Test06Performance : MonoBehaviour
{
    public int startCount = 2000;
    public int spawnCount = 1000;

    private PhysicsWorld m_World;
    private NativeList<PhysicsBody> m_Bodies;
    private Random m_Random = new(1234);
    private float m_SmoothedFrameTime;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        m_Bodies = new NativeList<PhysicsBody>(Allocator.Persistent);

        CreateContainer();
        SpawnBodies(startCount);
    }

    private void OnDestroy()
    {
        if (m_Bodies.IsCreated)
            m_Bodies.Dispose();
    }

    // A static U-shaped container to hold all the bodies.
    private void CreateContainer()
    {
        var container = m_World.CreateBody();
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        container.CreateShape(PolygonGeometry.CreateBox(new Vector2(30f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, -0.5f))), shapeDef);
        container.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 40f), radius: 0f, new PhysicsTransform(new Vector2(-15f, 20f))), shapeDef);
        container.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 40f), radius: 0f, new PhysicsTransform(new Vector2(15f, 20f))), shapeDef);
    }

    private void SpawnBodies(int count)
    {
        // 1. Create all the bodies in one call.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        using var bodies = m_World.CreateBodyBatch(bodyDef, count, Allocator.Temp);

        // 2. Give every body a random start position, then set them all in one call.
        var transforms = new NativeArray<PhysicsBody.BatchTransform>(count, Allocator.Temp);
        for (var i = 0; i < count; i++)
        {
            transforms[i] = new PhysicsBody.BatchTransform
            {
                physicsBody = bodies[i],
                position = new Vector2(m_Random.NextFloat(-13f, 13f), m_Random.NextFloat(15f, 40f)),
                rotation = PhysicsRotate.FromDegrees(m_Random.NextFloat(0f, 360f))
            };
        }
        PhysicsBody.SetBatchTransform(transforms);
        transforms.Dispose();

        // 3. Add a small shape to each body: circles and boxes.
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        for (var i = 0; i < count; i++)
        {
            if (i % 2 == 0)
            {
                shapeDef.surfaceMaterial.customColor = Color.orange;
                bodies[i].CreateShape(new CircleGeometry { radius = 0.15f }, shapeDef);
            }
            else
            {
                shapeDef.surfaceMaterial.customColor = Color.cornflowerBlue;
                bodies[i].CreateShape(PolygonGeometry.CreateBox(new Vector2(0.3f, 0.3f), radius: 0f, PhysicsTransform.identity), shapeDef);
            }

            m_Bodies.Add(bodies[i]);
        }
    }

    // Remove every spawned body in one call.
    private void RemoveAll()
    {
        PhysicsWorld.DestroyBodyBatch(m_Bodies.AsArray());
        m_Bodies.Clear();
    }

    private void Update()
    {
        m_SmoothedFrameTime = Mathf.Lerp(m_SmoothedFrameTime, Time.unscaledDeltaTime, 0.05f);

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.spaceKey.wasPressedThisFrame)
            SpawnBodies(spawnCount);

        if (keyboard.cKey.wasPressedThisFrame)
            RemoveAll();
    }

    // Show the numbers that matter.
    /*
    private void OnGUI()
    {
        var counters = m_World.counters;
        var profile = m_World.profile;

        GUI.Label(new Rect(10, 10, 600, 25), $"Bodies: {counters.bodyCount}    Contacts: {counters.contactCount}");
        GUI.Label(new Rect(10, 30, 600, 25), $"Physics step: {profile.simulationStep:F2} ms    Worker threads allowed: {m_World.simulationWorkers}");
        GUI.Label(new Rect(10, 50, 600, 25), $"Frame: {m_SmoothedFrameTime * 1000f:F1} ms ({1f / Mathf.Max(m_SmoothedFrameTime, 0.0001f):F0} FPS)");
        GUI.Label(new Rect(10, 70, 600, 25), $"[SPACE] add {spawnCount} bodies    [C] remove all");
    }
    */
}
