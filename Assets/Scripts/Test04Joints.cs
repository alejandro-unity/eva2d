using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test04: the main joint types, built in code.
///   1. Hinge    - a pendulum swinging around a pin.
///   2. Distance - a box hanging on a spring.
///   3. Slider   - a box moved left and right by a motor, between two limits.
///   4. Wheel    - a small car: two wheels with suspension and a motor.
///   5. Breakable bridge - planks joined by hinges. Joints have no "break force",
///      so we read each joint's force and destroy it ourselves when it is too high.
/// Press SPACE to drop a heavy box onto the bridge.
/// </summary>
public class Test04Joints : MonoBehaviour
{
    public float bridgeBreakForce = 2000f;

    private PhysicsWorld m_World;
    private PhysicsBody m_Ground;
    private PhysicsSliderJoint m_Slider;
    private PhysicsWheelJoint m_RearWheel;
    private PhysicsHingeJoint[] m_BridgeJoints;
    private float m_DirectionTimer;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // A static ground body. Joints that pin things "to the world" use it as body A.
        m_Ground = m_World.CreateBody();
        m_Ground.CreateShape(new SegmentGeometry { point1 = new Vector2(-14f, 0f), point2 = new Vector2(14f, 0f) }, PhysicsShapeDefinition.defaultDefinition);

        CreatePendulum();
        CreateSpring();
        CreateSlider();
        CreateCar();
        CreateBridge();
    }

    // 1. Hinge: a long box pinned at one end, so it swings.
    private void CreatePendulum()
    {
        var pin = new Vector2(-11f, 8f);
        var arm = CreateBox(new Vector2(-9f, 8f), new Vector2(4f, 0.3f), Color.orange);

        m_World.CreateJoint(new PhysicsHingeJointDefinition
        {
            bodyA = m_Ground,
            bodyB = arm,
            localAnchorA = m_Ground.GetLocalPoint(pin),
            localAnchorB = arm.GetLocalPoint(pin)
        });
    }

    // 2. Distance with a spring: a box that bounces up and down under a fixed point.
    private void CreateSpring()
    {
        var top = new Vector2(-5f, 9f);
        var box = CreateBox(new Vector2(-5f, 5f), new Vector2(1f, 1f), Color.cornflowerBlue);

        m_World.CreateJoint(new PhysicsDistanceJointDefinition
        {
            bodyA = m_Ground,
            bodyB = box,
            localAnchorA = m_Ground.GetLocalPoint(top),
            localAnchorB = PhysicsTransform.identity,
            distance = 3f,
            enableSpring = true,
            springFrequency = 1.5f,
            springDamping = 0.1f
        });
    }

    // 3. Slider with a motor and limits: the box moves along one axis only.
    private void CreateSlider()
    {
        var center = new Vector2(1f, 7f);
        var box = CreateBox(center, new Vector2(1f, 1f), Color.limeGreen);

        m_Slider = m_World.CreateJoint(new PhysicsSliderJointDefinition
        {
            bodyA = m_Ground,
            bodyB = box,
            localAnchorA = m_Ground.GetLocalPoint(center), // the slide axis is horizontal (no rotation)
            localAnchorB = PhysicsTransform.identity,
            enableLimit = true,
            lowerTranslationLimit = -2.5f,
            upperTranslationLimit = 2.5f,
            enableMotor = true,
            motorSpeed = 2f,
            maxMotorForce = 500f
        });
    }

    // 4. Wheel: a chassis with two wheels. The rear wheel has the motor.
    private void CreateCar()
    {
        var chassis = CreateBox(new Vector2(-6f, 1.2f), new Vector2(3f, 0.5f), Color.violet);
        var rearWheel = CreateBall(new Vector2(-7f, 0.5f), 0.45f, Color.gray);
        var frontWheel = CreateBall(new Vector2(-5f, 0.5f), 0.45f, Color.gray);

        m_RearWheel = CreateWheelJoint(chassis, rearWheel, withMotor: true);
        CreateWheelJoint(chassis, frontWheel, withMotor: false);
    }

    private PhysicsWheelJoint CreateWheelJoint(PhysicsBody chassis, PhysicsBody wheel, bool withMotor)
    {
        // The suspension axis is vertical: rotate the chassis anchor by 90 degrees.
        var axis = PhysicsRotate.FromDegrees(90f);

        return m_World.CreateJoint(new PhysicsWheelJointDefinition
        {
            bodyA = chassis,
            bodyB = wheel,
            localAnchorA = new PhysicsTransform(chassis.GetLocalPoint(wheel.position), axis),
            localAnchorB = PhysicsTransform.identity,
            enableSpring = true,
            springFrequency = 4f,
            springDamping = 0.7f,
            enableMotor = withMotor,
            motorSpeed = -360f, // degrees per second (negative = clockwise = drive right)
            maxMotorTorque = 50f
        });
    }

    // 5. A bridge of planks joined by hinges, pinned at both ends.
    private void CreateBridge()
    {
        const int plankCount = 8;
        var start = new Vector2(4f, 4f);
        var plankSize = new Vector2(1f, 0.25f);

        m_BridgeJoints = new PhysicsHingeJoint[plankCount + 1];
        var previous = m_Ground;

        for (var i = 0; i <= plankCount; i++)
        {
            var pin = start + new Vector2(i * plankSize.x, 0f);

            // The last joint pins the final plank back to the ground.
            var next = i < plankCount
                ? CreateBox(pin + new Vector2(plankSize.x * 0.5f, 0f), plankSize, Color.burlywood)
                : m_Ground;

            m_BridgeJoints[i] = m_World.CreateJoint(new PhysicsHingeJointDefinition
            {
                bodyA = previous,
                bodyB = next,
                localAnchorA = previous.GetLocalPoint(pin),
                localAnchorB = next.GetLocalPoint(pin)
            });

            previous = next;
        }
    }

    private void Update()
    {
        // Reverse the slider and the car every 3 seconds.
        m_DirectionTimer += Time.deltaTime;
        if (m_DirectionTimer > 3f)
        {
            m_DirectionTimer = 0f;
            m_Slider.motorSpeed = -m_Slider.motorSpeed;
            m_RearWheel.motorSpeed = -m_RearWheel.motorSpeed;
        }

        // SPACE drops a heavy box onto the bridge.
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            CreateBox(new Vector2(8f, 9f), new Vector2(1.2f, 1.2f), Color.red, density: 40f);

        BreakOverloadedBridgeJoints();
    }

    // Read each bridge joint's force. If the strongest one is too high, destroy it.
    // (Only one joint breaks at a time, like a real bridge snapping at its weakest point.)
    private void BreakOverloadedBridgeJoints()
    {
        var strongest = -1;
        var strongestForce = 0f;

        for (var i = 0; i < m_BridgeJoints.Length; i++)
        {
            if (!m_BridgeJoints[i].isValid)
                continue;

            var force = m_BridgeJoints[i].currentConstraintForce.magnitude;
            if (force > strongestForce)
            {
                strongest = i;
                strongestForce = force;
            }
        }

        if (strongest >= 0 && strongestForce > bridgeBreakForce)
        {
            Debug.Log($"Bridge joint {strongest} broke at {strongestForce:F0} N.");
            m_BridgeJoints[strongest].Destroy();
        }
    }

    // --- Small helpers to create dynamic bodies ---

    private PhysicsBody CreateBox(Vector2 position, Vector2 size, Color color, float density = 1f)
    {
        var body = CreateDynamicBody(position);
        body.CreateShape(PolygonGeometry.CreateBox(size, radius: 0f, PhysicsTransform.identity), ShapeDefinition(color, density));
        return body;
    }

    private PhysicsBody CreateBall(Vector2 position, float radius, Color color)
    {
        var body = CreateDynamicBody(position);
        body.CreateShape(new CircleGeometry { radius = radius }, ShapeDefinition(color, 1f));
        return body;
    }

    private PhysicsBody CreateDynamicBody(Vector2 position)
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = position;
        return m_World.CreateBody(bodyDef);
    }

    private static PhysicsShapeDefinition ShapeDefinition(Color color, float density)
    {
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.density = density;
        shapeDef.surfaceMaterial.customColor = color;
        return shapeDef;
    }
}
