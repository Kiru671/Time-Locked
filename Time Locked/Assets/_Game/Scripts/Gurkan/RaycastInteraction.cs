using UnityEngine;

public class RaycastInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionLayer;

    private Camera cam;
    private IInteractable currentInteractable;
    private Outline lastOutline;


    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // If camera is not yet found, try to find it..
        if (cam == null)
        {
            cam = Camera.main;
            // If we still can't find it, return and try again next frame. This prevents the null reference.
            if (cam == null) return;
        }
        
        
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            var outline = hit.collider.GetComponent<Outline>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                UIManager.Instance.ShowHint(interactable.GetInteractionText());

                // Outline aç
                if (outline != null && outline != lastOutline)
                {
                    if (lastOutline != null) lastOutline.enabled = false;
                    outline.enabled = true;
                    lastOutline = outline;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    var playerInventory = GetComponent<PlayerInventory>();
                    interactable.Interact(playerInventory);
                    UIManager.Instance.HideHint();
                }
            }
            else
            {
                ClearHint();
            }
        }
        else
        {
            ClearHint();
        }
    }

    private void ClearHint()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            UIManager.Instance.HideHint();
        }

        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }
    }
}
