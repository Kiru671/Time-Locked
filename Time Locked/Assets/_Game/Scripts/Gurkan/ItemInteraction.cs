using UnityEngine;

public class ItemInteraction : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    public string interactionText = "Press E to take";

    public string GetInteractionText()
    {
        return $"{interactionText} '{itemData.itemName}'";
    }

    public void Interact(PlayerInventory player)
    {
        player.TryAddItem(itemData);
        Destroy(gameObject);
    }
}
