using UnityEngine;
using Unity.U2D.Physics;

/// <summary>
/// Test01: the basic "Hello Physics" example, built entirely in code.
/// A static ground, a falling ball, a stack of boxes and a capsule.
/// No GameObjects or renderers are needed: the physics debug renderer draws every shape.
/// </summary>
public class Test01Basic : MonoBehaviour
{
    private void Start()
    {
        var world = PhysicsWorld.defaultWorld;

        // Draw every body, shape and joint, not only the selected ones.
        // Bodies created in code have no GameObject, so they can never be "selected".
        world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Ground: a static body (the default type) with a segment shape.
        var ground = world.CreateBody();
        ground.CreateShape(
            new SegmentGeometry { point1 = new Vector2(-20f, 0f), point2 = new Vector2(20f, 0f) },
            PhysicsShapeDefinition.defaultDefinition);

        // Every falling object is a dynamic body.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;

        var shapeDef = PhysicsShapeDefinition.defaultDefinition;

        // A falling ball.
        bodyDef.position = new Vector2(-3f, 5f);
        shapeDef.surfaceMaterial.customColor = Color.orange;
        world.CreateBody(bodyDef).CreateShape(new CircleGeometry { radius = 0.5f }, shapeDef);

        // A stack of boxes.
        shapeDef.surfaceMaterial.customColor = Color.cornflowerBlue;
        for (var i = 0; i < 5; ++i)
        {
            bodyDef.position = new Vector2(0f, 0.5f + i * 1.05f);
            world.CreateBody(bodyDef).CreateShape(PolygonGeometry.CreateBox(Vector2.one, radius: 0f, PhysicsTransform.identity), shapeDef);
        }

        // A capsule dropped off-centre onto the stack, so it tips over and falls off.
        bodyDef.position = new Vector2(0.9f, 8f);
        shapeDef.surfaceMaterial.customColor = Color.limeGreen;
        world.CreateBody(bodyDef).CreateShape(new CapsuleGeometry { center1 = new Vector2(-0.75f, 0f), center2 = new Vector2(0.75f, 0f), radius = 0.3f }, shapeDef);
    }
}
