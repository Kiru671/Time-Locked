using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using StarterAssets;

public class Raycast : MonoBehaviour
{
    [Header("Raycast Features")] [SerializeField]
    private float rayLength = 5;
    
    [Header("Crosshair")] [SerializeField] private Image crosshair;

    [Header("Input Key")] [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    private Camera _camera;
    private NoteController _noteController;
    
    private NetworkObject _rootNetworkObject;

    void Start()
    {
        // Ensure crosshair starts as white (idle)
        if (crosshair != null)
        {
            crosshair.color = Color.white;
        }
        else
        {
            // Try to find a crosshair Image in scene (tag it as "Crosshair" in the Canvas)
            var crosshairObj = GameObject.FindWithTag("Crosshair");
            if (crosshairObj != null)
            {
                crosshair = crosshairObj.GetComponent<Image>();
                if (crosshair != null) crosshair.color = Color.white;
            }
        }
        _rootNetworkObject = transform.root.GetComponent<NetworkObject>();
        // If the script is on the camera itself use that, otherwise look for a child camera.
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }

        // Fallback: use main camera if still not found (works when the Player only owns a virtual camera)
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // In case we didn't have a camera during Start (e.g., it was instantiated later), keep trying.
        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null) return; // still not ready
        }
        Ray centerRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(centerRay, out RaycastHit hit, rayLength))
        {
            var readableItem = hit.collider != null ? hit.collider.GetComponent<NoteController>() : null;
            if (readableItem != null)
            {
                _noteController = readableItem;
                HighlightCrosshair(true);
            }
            else
            {
                ClearNote();
            }
        }
        else
        {
            ClearNote();
        }

        if (_noteController != null && Input.GetKeyDown(interactKey))
        {
            FirstPersonController fpc = GetComponentInParent<FirstPersonController>();

            // Fallback: try to fetch from local player object if camera is detached
            if (fpc == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
            {
                var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (playerObj != null)
                {
                    fpc = playerObj.GetComponent<FirstPersonController>();
                }
            }

            _noteController.ShowNote(fpc); // Will still work even if fpc is null
        }
    }

    void ClearNote()
    {
        if (_noteController != null)
        {
            HighlightCrosshair(false);
            _noteController = null;
        }
    }

    void HighlightCrosshair(bool on)
    {
        if (on)
        {
            crosshair.color = Color.red;
        }
        else
        {
            crosshair.color = Color.white;
        }
    }
}
