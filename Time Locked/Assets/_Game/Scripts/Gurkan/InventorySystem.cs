using UnityEngine;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    public InventoryItemData[] slots = new InventoryItemData[4];

    public bool AddItem(InventoryItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                return true;
            }
        }
        return false; // Envanter dolu
    }

    public bool HasItem(string itemName)
    {
        return slots.Any(item => item != null && item.itemName == itemName);
    }

    public void RemoveItem(string itemName)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemName == itemName)
            {
                slots[i] = null;
                return;
            }
        }
    }
}
