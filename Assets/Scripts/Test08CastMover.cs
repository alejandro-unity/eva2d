using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test08: a character controller built with the Cast Mover.
/// The character is NOT a physics body. It is just a capsule shape that we move ourselves:
/// every step, CastMover sweeps the capsule towards where we want it to go,
/// stops it at walls, slides it along slopes, and returns where it ended up.
///   LEFT / RIGHT arrows (or A / D) = walk
///   SPACE                          = jump
/// The capsule is orange when it stands on something, and blue in the air.
/// </summary>
public class Test08CastMover : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float jumpSpeed = 10f;
    public float gravity = -25f;

    private PhysicsWorld m_World;

    // The character: a capsule, where it is, and how fast it moves.
    private readonly CapsuleGeometry m_Capsule = new() { center1 = new Vector2(0f, -0.5f), center2 = new Vector2(0f, 0.5f), radius = 0.35f };
    private PhysicsTransform m_Transform = new(new Vector2(-9f, 3f));
    private Vector2 m_Velocity;
    private bool m_OnGround;
    private float m_JumpTimer; // time left during which the ground is ignored, right after a jump

    // Input, read in Update and used in the physics step.
    private float m_MoveInput;
    private bool m_JumpRequested;

    private void OnEnable() => PhysicsEvents.PreSimulate += MoveCharacter;
    private void OnDisable() => PhysicsEvents.PreSimulate -= MoveCharacter;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;
        CreateLevel();
    }

    // A small level: ground, a slope, platforms, a wall and some loose boxes.
    private void CreateLevel()
    {
        var level = m_World.CreateBody();
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.surfaceMaterial.customColor = Color.gray;

        level.CreateShape(PolygonGeometry.CreateBox(new Vector2(26f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, -0.5f))), shapeDef);
        level.CreateShape(PolygonGeometry.CreateBox(new Vector2(6f, 0.4f), radius: 0f, new PhysicsTransform(new Vector2(-4f, 1.2f), PhysicsRotate.FromDegrees(20f))), shapeDef);
        level.CreateShape(PolygonGeometry.CreateBox(new Vector2(3f, 0.4f), radius: 0f, new PhysicsTransform(new Vector2(1.5f, 2.2f))), shapeDef);
        level.CreateShape(PolygonGeometry.CreateBox(new Vector2(3f, 0.4f), radius: 0f, new PhysicsTransform(new Vector2(5.5f, 4f))), shapeDef);
        level.CreateShape(PolygonGeometry.CreateBox(new Vector2(0.5f, 3f), radius: 0f, new PhysicsTransform(new Vector2(12.5f, 1.5f))), shapeDef);

        // Loose dynamic boxes. The character can stand on them.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        shapeDef.surfaceMaterial.customColor = Color.cornflowerBlue;
        for (var i = 0; i < 3; i++)
        {
            bodyDef.position = new Vector2(9f, 0.5f + i * 1.05f);
            m_World.CreateBody(bodyDef).CreateShape(PolygonGeometry.CreateBox(Vector2.one, radius: 0f, PhysicsTransform.identity), shapeDef);
        }
    }

    private void Update()
    {
        // Read the input every frame.
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            m_MoveInput = 0f;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) m_MoveInput -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) m_MoveInput += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) m_JumpRequested = true;
        }

        // Draw the character (it has no shape in the world, so we draw it ourselves).
        m_World.DrawGeometry(m_Capsule, m_Transform, m_OnGround ? Color.orange : Color.aquamarine);
    }

    // Called just before every physics step.
    private void MoveCharacter(PhysicsWorld world, float deltaTime)
    {
        // 1. Is there ground just below the character's feet?
        //    Right after a jump the feet are still close to the ground, so ignore it for a moment,
        //    or the jump would be cancelled on its first step.
        m_JumpTimer -= deltaTime;
        m_OnGround = m_JumpTimer <= 0f && IsOnGround(world);

        // 2. Decide the velocity we want.
        m_Velocity.x = m_MoveInput * walkSpeed;

        if (m_OnGround && m_JumpRequested)
        {
            m_Velocity.y = jumpSpeed;   // jump (works on flat ground and on slopes)
            m_JumpTimer = 0.2f;
        }
        else if (m_OnGround)
        {
            m_Velocity.y = 0f;          // standing: no gravity, so we don't slide down slopes
        }
        else
        {
            m_Velocity.y += gravity * deltaTime; // in the air: fall
        }

        m_JumpRequested = false;

        // 3. Ask the Cast Mover to move the capsule there, colliding with the world.
        var input = new PhysicsQuery.WorldMoverInput
        {
            geometry = m_Capsule,
            transform = m_Transform,
            targetPosition = m_Transform.position + m_Velocity * deltaTime,
            velocity = m_Velocity,
            maxIterations = 5,
            moveTolerance = 0.01f,
            overlapFilter = PhysicsQuery.QueryFilter.Everything,
            castFilter = PhysicsQuery.QueryFilter.Everything
        };

        var result = world.CastMover(input);

        // 4. Use the result: the new position, and the velocity after hitting things.
        m_Transform = result.transform;
        m_Velocity = result.velocity;

        // Fell off the level? Start again.
        if (m_Transform.position.y < -10f)
        {
            m_Transform = new PhysicsTransform(new Vector2(-9f, 3f));
            m_Velocity = Vector2.zero;
        }
    }

    // Cast a small circle down from the bottom of the capsule.
    // If it hits a surface that faces upwards within a few centimetres, we are standing on it.
    private bool IsOnGround(PhysicsWorld world)
    {
        var feet = new CircleGeometry
        {
            center = m_Transform.TransformPoint(m_Capsule.center1),
            radius = m_Capsule.radius * 0.9f
        };

        using var hits = world.CastShapeProxy(new PhysicsShape.ShapeProxy(feet), new Vector2(0f, -0.1f), PhysicsQuery.QueryFilter.Everything);
        return hits.Length > 0 && hits[0].normal.y > 0.7f;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 600, 25), "[LEFT / RIGHT] or [A / D] walk    [SPACE] jump");
        GUI.Label(new Rect(10, 30, 600, 25), m_OnGround ? "On the ground" : "In the air");
    }
}
