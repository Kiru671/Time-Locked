using UnityEngine;

public class Portal2 : MonoBehaviour
{
    [SerializeField] private Portal2 linkedPortal;
    [SerializeField] private MeshRenderer screen;
    
    private Camera playerCam;
    private Camera portalCam;
    private RenderTexture viewTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;
    }
    

    void CreateViewTexture()
    {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null)
                viewTexture.Release();

            viewTexture = new RenderTexture(Screen.width, Screen.height, 24);

            portalCam.targetTexture = viewTexture;
            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public void Render()
    {
        if (!VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            return;
        }
        screen.enabled = false;
        CreateViewTexture();

        Matrix4x4 m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix *
                playerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3),m.rotation);
        
        portalCam.Render();

        screen.enabled = true;
    }
    
    

    // Update is called once per frame
    void Update()
    {
        Render();
    }
}