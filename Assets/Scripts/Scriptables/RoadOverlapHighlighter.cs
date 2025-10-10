using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class RoadCollisionHighlighter : MonoBehaviour
{
    [Header("Color Settings")]
    public Color overlapColor = Color.red;
    public float colorLerpSpeed = 10f; // Smooth color transition speed

    private Renderer[] segmentRenderers;
    private Color[][] originalColors;

    private bool isDragging = false;
    private bool isOverlapping = false;

    private readonly HashSet<Collider> currentOverlaps = new HashSet<Collider>();

    void Awake()
    {
        // Collect renderers only from segment children
        List<Renderer> renderers = new List<Renderer>();
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("segment"))
                renderers.AddRange(child.GetComponentsInChildren<Renderer>());
        }

        segmentRenderers = renderers.ToArray();
        CacheOriginalColors();

        // Ensure physics setup
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        // Smoothly transition between colors
        for (int i = 0; i < segmentRenderers.Length; i++)
        {
            Material[] mats = segmentRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].HasProperty("_Color"))
                {
                    Color targetColor = isOverlapping ? overlapColor : originalColors[i][j];
                    mats[j].color = Color.Lerp(mats[j].color, targetColor, Time.deltaTime * colorLerpSpeed);
                }
            }
        }
    }

    public void SetDragging(bool dragging)
    {
        isDragging = dragging;

        // If drag stops but still overlapping, stay red
        if (!dragging && currentOverlaps.Count > 0)
            isOverlapping = true;
        else if (!dragging)
            isOverlapping = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || other.gameObject == gameObject) return;
        if (other.gameObject.layer != gameObject.layer) return;

        currentOverlaps.Add(other);
        isOverlapping = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || other.gameObject == gameObject) return;
        if (other.gameObject.layer != gameObject.layer) return;

        currentOverlaps.Remove(other);
        if (currentOverlaps.Count == 0)
            isOverlapping = false;
    }

    // --- Helpers ---
    private void CacheOriginalColors()
    {
        originalColors = new Color[segmentRenderers.Length][];
        for (int i = 0; i < segmentRenderers.Length; i++)
        {
            Material[] mats = segmentRenderers[i].materials;
            originalColors[i] = new Color[mats.Length];
            for (int j = 0; j < mats.Length; j++)
                if (mats[j].HasProperty("_Color"))
                    originalColors[i][j] = mats[j].color;
        }
    }
}
