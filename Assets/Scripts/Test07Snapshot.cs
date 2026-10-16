using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test07: save and rewind the whole physics world (snapshots), and check that it is deterministic.
/// A ball is thrown at a pyramid of boxes. A snapshot of the world is taken at the start.
///   R = rewind the world to the snapshot, and watch the exact same crash again
///   S = take a new snapshot now
/// 100 steps after each snapshot or rewind, the positions of all bodies are turned into one number (a hash).
/// If the replay is identical, the hash matches the first run.
/// </summary>
public class Test07Snapshot : MonoBehaviour
{
    private const int StepsToCompare = 100;

    private PhysicsWorld m_World;
    private PhysicsWorld.Snapshot m_Snapshot;
    private readonly List<PhysicsBody> m_Bodies = new();

    private int m_StepsSinceSnapshot;
    private ulong? m_FirstRunHash;
    private string m_Result = "Waiting for the first run...";

    private void OnEnable() => PhysicsEvents.PostSimulate += OnPostSimulate;
    private void OnDisable() => PhysicsEvents.PostSimulate -= OnPostSimulate;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        CreateScene();
        TakeSnapshot();
    }

    private void OnDestroy()
    {
        if (m_Snapshot.IsCreated)
            m_Snapshot.Dispose();
    }

    private void CreateScene()
    {
        // Ground.
        var ground = m_World.CreateBody();
        ground.CreateShape(new SegmentGeometry { point1 = new Vector2(-20f, 0f), point2 = new Vector2(20f, 0f) }, PhysicsShapeDefinition.defaultDefinition);

        // A pyramid of boxes.
        const int rows = 8;
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < rows - row; column++)
            {
                var x = -1f + column * 1.05f + row * 0.525f;
                var y = 0.5f + row * 1.05f;
                m_Bodies.Add(CreateBody(new Vector2(x, y), Vector2.zero, isBall: false));
            }
        }

        // A heavy ball, thrown at the pyramid.
        m_Bodies.Add(CreateBody(new Vector2(-10f, 3f), new Vector2(18f, 4f), isBall: true));
    }

    private PhysicsBody CreateBody(Vector2 position, Vector2 velocity, bool isBall)
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = position;
        bodyDef.linearVelocity = velocity;
        var body = m_World.CreateBody(bodyDef);

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        if (isBall)
        {
            shapeDef.density = 5f;
            shapeDef.surfaceMaterial.customColor = Color.orange;
            body.CreateShape(new CircleGeometry { radius = 0.6f }, shapeDef);
        }
        else
        {
            shapeDef.surfaceMaterial.customColor = Color.cornflowerBlue;
            body.CreateShape(PolygonGeometry.CreateBox(Vector2.one, radius: 0f, PhysicsTransform.identity), shapeDef);
        }

        return body;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame)
            Rewind();

        if (keyboard.sKey.wasPressedThisFrame)
            TakeSnapshot();
    }

    // Save the full state of the world: bodies, shapes, joints, contacts, everything.
    private void TakeSnapshot()
    {
        if (m_Snapshot.IsCreated)
            m_Snapshot.Dispose();

        m_Snapshot = m_World.CreateSnapshot(Allocator.Persistent);
        m_StepsSinceSnapshot = 0;
        m_FirstRunHash = null;
        m_Result = $"Snapshot taken ({m_Snapshot.Length / 1024f:F1} KB). Press R to rewind.";
    }

    // Put the world back exactly as it was when the snapshot was taken.
    // The body handles we keep in m_Bodies stay valid.
    private void Rewind()
    {
        if (!m_Snapshot.IsCreated)
            return;

        m_World.ApplySnapshot(m_Snapshot);

        // The snapshot restores the simulation, but world settings such as drawing go back to the
        // project defaults, so set the draw options again.
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        m_StepsSinceSnapshot = 0;
        m_Result = "Rewound. Replaying...";
    }

    // Count the steps after a snapshot or rewind, and compare the world state after StepsToCompare steps.
    private void OnPostSimulate(PhysicsWorld world, float timeStep)
    {
        m_StepsSinceSnapshot++;
        if (m_StepsSinceSnapshot != StepsToCompare)
            return;

        var hash = HashAllBodies();

        if (m_FirstRunHash == null)
        {
            m_FirstRunHash = hash;
            m_Result = $"First run recorded: hash {hash:X16}. Press R to rewind and compare.";
        }
        else
        {
            var same = hash == m_FirstRunHash;
            m_Result = same
                ? $"Replay IDENTICAL to the first run (hash {hash:X16})."
                : $"Replay DIFFERENT: {hash:X16} vs {m_FirstRunHash:X16}.";
            Debug.Log(m_Result);
        }
    }

    // Turn every body's exact position and rotation into one number.
    private ulong HashAllBodies()
    {
        ulong hash = 14695981039346656037; // FNV-1a
        foreach (var body in m_Bodies)
        {
            hash = Mix(hash, body.position.x);
            hash = Mix(hash, body.position.y);
            hash = Mix(hash, body.rotation.direction.x);
            hash = Mix(hash, body.rotation.direction.y);
        }
        return hash;
    }

    private static ulong Mix(ulong hash, float value)
    {
        hash ^= (uint)System.BitConverter.SingleToInt32Bits(value);
        return hash * 1099511628211;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 800, 25), "[R] Rewind to the snapshot    [S] Take a new snapshot now");
        GUI.Label(new Rect(10, 30, 800, 25), $"Steps since snapshot: {m_StepsSinceSnapshot}");
        GUI.Label(new Rect(10, 50, 800, 25), m_Result);
    }
}
