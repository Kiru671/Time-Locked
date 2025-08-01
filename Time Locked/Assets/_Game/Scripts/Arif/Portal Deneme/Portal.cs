using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
    [Tooltip("The other end of this portal pair")]
    public Portal linkedPortal;

    [Header("Render Texture Settings")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;

    [HideInInspector] public Camera portalCam;
    [HideInInspector] public RenderTexture viewTexture;
    MeshRenderer screenRenderer;
    public MeshRenderer Renderer => screenRenderer;

    void Awake()
    {
        screenRenderer = GetComponent<MeshRenderer>();

        // Create RT
        viewTexture = new RenderTexture(textureWidth, textureHeight, 24);
        viewTexture.name = $"{name}_RT";

        // Create offscreen camera
        GameObject go = new GameObject($"{name}_Cam");
        go.transform.parent = transform;
        portalCam = go.AddComponent<Camera>();
        portalCam.enabled = false;
        portalCam.targetTexture = viewTexture;

        // Assign RT to portal surface material
        screenRenderer.material = new Material(screenRenderer.sharedMaterial);
        screenRenderer.material.SetTexture("_MainTex", viewTexture);
    }

    void OnDestroy()
    {
        if (viewTexture) viewTexture.Release();
        if (portalCam) Destroy(portalCam.gameObject);
    }
}