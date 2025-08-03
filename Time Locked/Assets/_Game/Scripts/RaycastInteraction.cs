using UnityEngine;

public class RaycastInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionLayer;

    private Camera cam;
    private IInteractable currentInteractable;
    // private Outline lastOutline; // QuickOutline asset'i eksik olduğu için devre dışı


    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            // var outline = hit.collider.GetComponent<Outline>(); // QuickOutline asset'i eksik

            if (interactable != null)
            {
                currentInteractable = interactable;
                UIManager.Instance.ShowHint(interactable.GetInteractionText());

                // Outline aç (QuickOutline asset'i eksik olduğu için devre dışı)
                // if (outline != null && outline != lastOutline)
                // {
                //     if (lastOutline != null) lastOutline.enabled = false;
                //     outline.enabled = true;
                //     lastOutline = outline;
                // }

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

        // Outline kapat (QuickOutline asset'i eksik olduğu için devre dışı)
        // if (lastOutline != null)
        // {
        //     lastOutline.enabled = false;
        //     lastOutline = null;
        // }
    }
}
