using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventorySystem inventorySystem;
    public InventoryUIController uiController;

    private void Start()
    {
        uiController.RefreshUI(inventorySystem.slots);
    }

    public void TryAddItem(InventoryItemData item)
    {
        if (inventorySystem.AddItem(item))
            uiController.RefreshUI(inventorySystem.slots);
    }

    public void TryRemoveItem(string itemName)
    {
        inventorySystem.RemoveItem(itemName);
        uiController.RefreshUI(inventorySystem.slots);
    }

    public bool HasItem(string itemName) => inventorySystem.HasItem(itemName);
}
