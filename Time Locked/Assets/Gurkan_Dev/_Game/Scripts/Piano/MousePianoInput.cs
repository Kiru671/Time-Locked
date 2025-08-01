using UnityEngine;

public class MousePianoInput : MonoBehaviour
{
    [Header("Settings")]
    public Camera MainCamera;
    public float ClickVelocity = 80f;
    public float ClickLength = 1f;
    public float ClickSpeed = 1f;
    
    [Header("Debug")]
    public LayerMask PianoKeyLayerMask = -1; // All layers by default
    
    void Start()
    {
        // If no camera is assigned, use the main camera
        if (MainCamera == null)
            MainCamera = Camera.main;
    }
    
    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            HandleMouseClick();
        }
    }
    
    void HandleMouseClick()
    {
        // Create a ray from camera through mouse position
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Perform raycast
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, PianoKeyLayerMask))
        {
            // Check if the hit object has a PianoKey component
            PianoKey pianoKey = hit.collider.GetComponent<PianoKey>();
            
            if (pianoKey != null)
            {
                // Play the piano key
                pianoKey.Play(ClickVelocity, ClickLength, ClickSpeed);
                
                // Optional: Debug log to see which key was pressed
                //Debug.Log($"Played piano key: {hit.collider.name}");
            }
        }
    }
} 