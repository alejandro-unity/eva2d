using UnityEngine;
using UnityEngine.InputSystem;
using Unity.U2D.Physics;

/// <summary>
/// Test03: sprites driven by physics, using components (Physics Pose + Physics Area Sprite).
/// The scene objects are set up in the Editor. This script only adds two keyboard switches,
/// to show that a sprite follows its body only when BOTH transform write switches are on.
///   W = toggle the WORLD Transform Write Mode (Off / Fast 2D)
///   P = toggle the POSE Transform Write Mode of every body (Off / Interpolate)
///   R = drop the sprites again
/// </summary>
public class Test03SpriteSwitches : MonoBehaviour
{
    [Tooltip("The dynamic sprites to control.")]
    public PhysicsPose[] poses;

    private Vector3[] m_StartPositions;

    private void Start()
    {
        // Also draw the physics outlines, so you can see the bodies keep moving when a sprite is frozen.
        var world = PhysicsWorld.defaultWorld;
        world.drawOptions = PhysicsWorld.DrawOptions.DefaultAll;

        // Remember where each sprite starts, so R can reset them.
        m_StartPositions = new Vector3[poses.Length];
        for (var i = 0; i < poses.Length; i++)
            m_StartPositions[i] = poses[i].transform.position;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.wKey.wasPressedThisFrame)
            ToggleWorldSwitch();

        if (keyboard.pKey.wasPressedThisFrame)
            TogglePoseSwitch();

        if (keyboard.rKey.wasPressedThisFrame)
            ResetSprites();
    }

    // Switch 1: the world decides if ANY body may write to its Transform.
    private void ToggleWorldSwitch()
    {
        var world = PhysicsWorld.defaultWorld;
        world.transformWriteMode = world.transformWriteMode == PhysicsWorld.TransformWriteMode.Off
            ? PhysicsWorld.TransformWriteMode.Fast2D
            : PhysicsWorld.TransformWriteMode.Off;
    }

    // Switch 2: each body decides if IT writes to its Transform.
    private void TogglePoseSwitch()
    {
        foreach (var pose in poses)
        {
            var body = pose.body;
            body.transformWriteMode = body.transformWriteMode == PhysicsBody.TransformWriteMode.Off
                ? PhysicsBody.TransformWriteMode.Interpolate
                : PhysicsBody.TransformWriteMode.Off;
        }
    }

    // Move every body back to its start position and stop it.
    private void ResetSprites()
    {
        for (var i = 0; i < poses.Length; i++)
        {
            var body = poses[i].body;
            body.position = m_StartPositions[i];
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    // Show the state of both switches on screen.
    private void OnGUI()
    {
        if (poses == null || poses.Length == 0)
            return;

        var worldMode = PhysicsWorld.defaultWorld.transformWriteMode;
        var poseMode = poses[0].body.transformWriteMode;
        var moving = worldMode != PhysicsWorld.TransformWriteMode.Off && poseMode != PhysicsBody.TransformWriteMode.Off;

        GUI.Label(new Rect(10, 10, 600, 25), $"[W] World Transform Write Mode: {worldMode}");
        GUI.Label(new Rect(10, 30, 600, 25), $"[P] Pose Transform Write Mode:  {poseMode}");
        GUI.Label(new Rect(10, 50, 600, 25), moving ? "Sprites follow their bodies." : "Sprites are FROZEN (the outlines still move).");
        GUI.Label(new Rect(10, 70, 600, 25), "[R] Drop the sprites again");
    }
}
