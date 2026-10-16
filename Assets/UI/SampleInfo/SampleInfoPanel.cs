using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.U2D.Physics;

/// <summary>
/// An on-screen panel (UI Toolkit) that lists the 2D Physics Core API a sample uses,
/// plus live numbers from the physics world. Fill in the fields in the Inspector.
/// Press I to hide or show the panel.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class SampleInfoPanel : MonoBehaviour
{
    public string title = "Sample";

    [TextArea(2, 4)]
    public string description;

    [Tooltip("One line per API or component used by this sample.")]
    public string[] apiUsed;

    private VisualElement m_Panel;
    private Label m_Counters;
    private float m_RefreshTimer;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        m_Panel = root.Q<VisualElement>("panel");
        m_Counters = root.Q<Label>("countersLabel");
        root.Q<Label>("titleLabel").text = title;
        root.Q<Label>("descriptionLabel").text = description;

        // One label per API line.
        var apiList = root.Q<VisualElement>("apiList");
        apiList.Clear();
        foreach (var api in apiUsed)
        {
            var item = new Label("• " + api);
            item.AddToClassList("sample-info-item");
            apiList.Add(item);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
            m_Panel.ToggleInClassList("sample-info-panel--hidden");

        // Refresh the live numbers four times a second.
        m_RefreshTimer -= Time.unscaledDeltaTime;
        if (m_RefreshTimer > 0f)
            return;
        m_RefreshTimer = 0.25f;

        var world = PhysicsWorld.defaultWorld;
        var counters = world.counters;
        m_Counters.text =
            $"Bodies {counters.bodyCount}  ·  Shapes {counters.shapeCount}  ·  Joints {counters.jointCount}\n" +
            $"Contacts {counters.contactCount}  ·  Physics step {world.profile.simulationStep:F2} ms";
    }
}
