using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventorySystem inventorySystem;
    public InventoryUIController uiController;

    private void Start()
    {
        // Null check ekleyelim
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }
        
        if (uiController == null)
        {
            uiController = GetComponent<InventoryUIController>();
        }
        
        if (uiController != null && inventorySystem != null)
        {
            uiController.RefreshUI(inventorySystem.slots);
        }
        else
        {
            Debug.LogWarning("PlayerInventory: inventorySystem veya uiController eksik!");
        }
    }

    public void TryAddItem(InventoryItemData item)
    {
        if (inventorySystem != null && inventorySystem.AddItem(item))
        {
            if (uiController != null)
                uiController.RefreshUI(inventorySystem.slots);
        }
    }

    public void TryRemoveItem(string itemName)
    {
        if (inventorySystem != null)
        {
            inventorySystem.RemoveItem(itemName);
            if (uiController != null)
                uiController.RefreshUI(inventorySystem.slots);
        }
    }

    public bool HasItem(string itemName) => inventorySystem.HasItem(itemName);
    
    // Belirli bir sınıfa ait item var mı kontrol et
    public bool HasItemOfClass(string itemClass)
    {
        if (inventorySystem == null || inventorySystem.slots == null) return false;
        
        foreach (var slot in inventorySystem.slots)
        {
            if (slot != null && slot.itemClass == itemClass)
            {
                return true;
            }
        }
        return false;
    }
    
    // Tetikleyici item'ları kontrol et
    public bool HasTriggerItem()
    {
        if (inventorySystem == null || inventorySystem.slots == null) return false;
        
        foreach (var slot in inventorySystem.slots)
        {
            if (slot != null && slot.isTriggerItem)
            {
                return true;
            }
        }
        return false;
    }
}
