using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    public InventoryItemData itemData;
    public string interactionText = "Press E to take";
    
    [Header("Pickup Settings")]
    public bool removeColliderOnPickup = true;
    public bool removeRigidbodyOnPickup = false;

    private bool isPickedUp = false;

    public string GetInteractionText()
    {
        return $"{interactionText} '{itemData.itemName}'";
    }

    public void Interact(PlayerInventory player)
    {
        if (isPickedUp) return;

        // Objeyi envantere ekle ve dünya objesini de geç
        if (player.TryAddItemWithWorldObject(itemData, gameObject))
        {
            PickupObject();
        }
    }

    private void PickupObject()
    {
        isPickedUp = true;
        
        // Collider'ı kaldır (opsiyonel)
        if (removeColliderOnPickup)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
        
        // Rigidbody'yi kaldır (opsiyonel)
        if (removeRigidbodyOnPickup)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
        
        // Objeyi deaktif et (destroy etme!)
        gameObject.SetActive(false);
        
        Debug.Log($"📦 Picked up {itemData.itemName} (object deactivated, not destroyed)");
    }

    // Objeyi tekrar dünyaya yerleştirme
    public void PlaceInWorld(Vector3 position, Quaternion rotation)
    {
        if (!isPickedUp) return;

        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(null);
        
        // Componenentleri geri aktif et
        if (removeColliderOnPickup)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }
        }
        
        if (removeRigidbodyOnPickup)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        
        gameObject.SetActive(true);
        isPickedUp = false;
        
        Debug.Log($"📍 Placed {itemData.itemName} back in world");
    }

    // Reset metodu (test için)
    public void ResetToOriginalState()
    {
        isPickedUp = false;
        gameObject.SetActive(true);
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}
