using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class CubeTriggerColor : MonoBehaviour
{
    [Header("Overlap Settings")]
    public Color overlapColor = Color.red; // Adjustable in Inspector

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the other object also uses this script
        if (other.GetComponent<CubeTriggerColor>() != null)
        {
            rend.material.color = overlapColor;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CubeTriggerColor>() != null)
        {
            rend.material.color = originalColor;
        }
    }
}
