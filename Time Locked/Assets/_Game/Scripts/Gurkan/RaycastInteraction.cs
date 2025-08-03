using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaycastInteraction : NetworkBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactionLayer;

    private Camera cam;
    private IInteractable currentInteractable;
    private Outline lastOutline;
    private PlayerInventory playerInventory;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.enabled = false;
        }
    }

    private IEnumerator GetPlayerInventory(IInteractable interactable)
    {
        while(playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
            if (playerInventory != null) break;
            yield return new WaitForSeconds(0.1f);
        }
        interactable.Interact(playerInventory);
        UIManager.Instance.HideHint();
    }
    
    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;
        if (cam == null)
        {
            cam = Camera.main;
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
                //UIManager.Instance.ShowHint(interactable.GetInteractionText());

                // Outline aç
                if (outline != null && outline != lastOutline)
                {
                    if (lastOutline != null) lastOutline.enabled = false;
                    outline.enabled = true;
                    lastOutline = outline;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("Pressed E");
                    StartCoroutine(GetPlayerInventory(interactable));
                }
            }
        }
        else if (Physics.Raycast(ray, out RaycastHit mirrorHit, interactionRange, LayerMask.GetMask("Mirror")))
        {
            UIManager.Instance.ShowHint("Press E to ");
            
            var interactable = mirrorHit.collider.GetComponent<Mirror>();
            if (Input.GetKeyDown(KeyCode.E))
            {
                var playerInventory = gameObject.GetComponent<PlayerInventory>();
                interactable.SendItem(playerInventory.heldItemManager.currentHeldNetworkItem.NetworkObjectId, playerInventory);
                UIManager.Instance.HideHint();
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
