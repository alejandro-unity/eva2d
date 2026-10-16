using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test05: asking the world questions (queries), using the mouse.
///   Ray cast:      a ray from the white dot (top left) to the mouse.
///                  Green = it hit a shape (with the hit point and surface normal). Red = nothing hit.
///   Overlap point: the shape under the mouse turns yellow.
///   Click:         push the shape under the mouse upwards.
/// Queries only READ the world, so they are also safe to run inside jobs.
/// </summary>
public class Test05Queries : MonoBehaviour
{
    private readonly Vector2 m_RayOrigin = new(-8f, 7f);

    private PhysicsWorld m_World;
    private PhysicsShape m_HighlightedShape;
    private Color m_HighlightedShapeColor;

    private void Start()
    {
        m_World = PhysicsWorld.defaultWorld;
        m_World.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Ground and two static obstacles.
        var ground = m_World.CreateBody();
        ground.CreateShape(PolygonGeometry.CreateBox(new Vector2(24f, 1f), radius: 0f, new PhysicsTransform(new Vector2(0f, -0.5f))), ShapeDefinition(Color.white));
        ground.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 4f), radius: 0f, new PhysicsTransform(new Vector2(-3f, 2f))), ShapeDefinition(Color.gray));
        ground.CreateShape(new CircleGeometry { center = new Vector2(3f, 6f), radius = 1f }, ShapeDefinition(Color.gray));

        // Some dynamic shapes to play with.
        for (var i = 0; i < 6; i++)
        {
            var body = CreateDynamicBody(new Vector2(-1f + i * 1.5f, 1f + i));
            if (i % 2 == 0)
                body.CreateShape(PolygonGeometry.CreateBox(new Vector2(1f, 1f), radius: 0.1f, PhysicsTransform.identity), ShapeDefinition(Color.cornflowerBlue));
            else
                body.CreateShape(new CircleGeometry { radius = 0.5f }, ShapeDefinition(Color.orange));
        }
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || Camera.main == null)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());

        RayCastToMouse(mousePosition);
        HighlightShapeUnderMouse(mousePosition);

        if (mouse.leftButton.wasPressedThisFrame)
            PushShapeUnderMouse(mousePosition);
    }

    // 1. Ray cast: find the closest shape along a line.
    private void RayCastToMouse(Vector2 mousePosition)
    {
        var ray = new PhysicsQuery.CastRayInput
        {
            origin = m_RayOrigin,
            translation = mousePosition - m_RayOrigin, // where the ray goes, relative to its origin
            maxFraction = 1f
        };

        m_World.DrawPoint(m_RayOrigin, 10f, Color.white);

        // "Closest" returns only the first shape the ray hits.
        using var hits = m_World.CastRay(ray, PhysicsQuery.QueryFilter.Everything, PhysicsQuery.WorldCastMode.Closest);

        if (hits.Length > 0)
        {
            var hit = hits[0];
            m_World.DrawLine(m_RayOrigin, hit.point, Color.green);
            m_World.DrawPoint(hit.point, 8f, Color.green);
            m_World.DrawLine(hit.point, hit.point + hit.normal, Color.yellow);
        }
        else
        {
            m_World.DrawLine(m_RayOrigin, mousePosition, Color.red);
        }
    }

    // 2. Overlap point: find the shape under a point.
    private void HighlightShapeUnderMouse(Vector2 mousePosition)
    {
        // Put the colour back on the shape we highlighted last frame.
        if (m_HighlightedShape.isValid)
            SetColor(m_HighlightedShape, m_HighlightedShapeColor);
        m_HighlightedShape = default;

        using var overlaps = m_World.OverlapPoint(mousePosition, PhysicsQuery.QueryFilter.Everything);
        if (overlaps.Length == 0)
            return;

        m_HighlightedShape = overlaps[0].shape;
        m_HighlightedShapeColor = m_HighlightedShape.surfaceMaterial.customColor;
        SetColor(m_HighlightedShape, Color.yellow);
    }

    // 3. Use a query result: push the dynamic body under the mouse.
    private void PushShapeUnderMouse(Vector2 mousePosition)
    {
        using var overlaps = m_World.OverlapPoint(mousePosition, PhysicsQuery.QueryFilter.Everything);
        if (overlaps.Length == 0)
            return;

        var body = overlaps[0].shape.body;
        if (body.type == PhysicsBody.BodyType.Dynamic)
            body.ApplyLinearImpulseToCenter(new Vector2(0f, 8f), true);
    }

    // --- Small helpers ---

    private PhysicsBody CreateDynamicBody(Vector2 position)
    {
        var bodyDef = PhysicsBodyDefinition.defaultDefinition;
        bodyDef.type = PhysicsBody.BodyType.Dynamic;
        bodyDef.position = position;
        return m_World.CreateBody(bodyDef);
    }

    private static PhysicsShapeDefinition ShapeDefinition(Color color)
    {
        var shapeDef = PhysicsShapeDefinition.defaultDefinition;
        shapeDef.surfaceMaterial.customColor = color;
        return shapeDef;
    }

    private static void SetColor(PhysicsShape shape, Color color)
    {
        var surfaceMaterial = shape.surfaceMaterial;
        surfaceMaterial.customColor = color;
        shape.surfaceMaterial = surfaceMaterial;
    }
}
