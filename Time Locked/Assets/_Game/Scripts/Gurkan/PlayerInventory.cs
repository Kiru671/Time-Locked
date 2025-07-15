using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventorySystem inventorySystem;

    private void Start()
    {
        if (InventoryUIController.Instance != null)
        {
            InventoryUIController.Instance.RefreshUI(inventorySystem.slots);
        }
    }

    public void TryAddItem(InventoryItemData item)
    {
        if (inventorySystem.AddItem(item))
            InventoryUIController.Instance?.RefreshUI(inventorySystem.slots);
    }

    public void TryRemoveItem(string itemName)
    {
        inventorySystem.RemoveItem(itemName);
        InventoryUIController.Instance?.RefreshUI(inventorySystem.slots);
    }

    public bool HasItem(string itemName) => inventorySystem.HasItem(itemName);
}
