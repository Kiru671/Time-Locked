using UnityEngine;

public class ChessTile : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Light highlightLight;
    public Renderer tileRenderer;
    public Material normalMaterial;
    public Material highlightMaterial;
    public Color highlightColor = Color.yellow;
    
    private Color originalColor;
    
    void Start()
    {
        if (tileRenderer == null)
            tileRenderer = GetComponent<Renderer>();
            
        if (tileRenderer != null)
            originalColor = tileRenderer.material.color;
    }

    public void Highlight(bool isOn)
    {
        // Light ile highlight
        if (highlightLight != null)
            highlightLight.enabled = isOn;
            
        // Material ile highlight
        if (tileRenderer != null)
        {
            if (isOn)
            {
                if (highlightMaterial != null)
                {
                    tileRenderer.material = highlightMaterial;
                }
                else
                {
                    tileRenderer.material.color = highlightColor;
                }
            }
            else
            {
                if (normalMaterial != null)
                {
                    tileRenderer.material = normalMaterial;
                }
                else
                {
                    tileRenderer.material.color = originalColor;
                }
            }
        }
    }
}
