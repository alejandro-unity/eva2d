using UnityEngine;

/// <summary>
/// Test09: a tiny target for a UnityEvent.
/// The Physics Area on this GameObject has Callback Source = Events,
/// and its "On Contact Begin" UnityEvent calls Flash() (wired in the Inspector, no physics code here).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Test09Flash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public int hitCount;

    private SpriteRenderer m_Renderer;
    private Color m_BaseColor;

    private void Awake()
    {
        m_Renderer = GetComponent<SpriteRenderer>();
        m_BaseColor = m_Renderer.color;
    }

    // Called by the Physics Area's "On Contact Begin" UnityEvent.
    public void Flash()
    {
        hitCount++;
        m_Renderer.color = flashColor;
    }

    // Fade back to the normal colour.
    private void Update()
    {
        m_Renderer.color = Color.Lerp(m_Renderer.color, m_BaseColor, Time.deltaTime * 8f);
    }
}
