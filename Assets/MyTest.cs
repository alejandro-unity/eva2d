using UnityEngine;
using Unity.U2D.Physics;

public class MyTest : MonoBehaviour
{
    

    void Start()
    {
        var world = PhysicsWorld.defaultWorld;

        // Draw every body, shape and joint, not only the selected ones.
        // Bodies created in code have no GameObject, so they can never be "selected".
        world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Suelo: un body estático con una forma de segmento.
        var ground = world.CreateBody();
        ground.CreateShape(
            new SegmentGeometry { point1 = new Vector2(-100, 0), point2 = new Vector2(100, 0) },
            PhysicsShapeDefinition.defaultDefinition);
    
        // Una pelota que cae.
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = new Vector2(0, 5);
        var ball = world.CreateBody(bodyDef);
        ball.CreateShape(new CircleGeometry { radius = 0.5f }, PhysicsShapeDefinition.defaultDefinition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
