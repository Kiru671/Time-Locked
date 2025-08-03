using UnityEngine;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    public InventoryItemData[] slots = new InventoryItemData[4];
    private bool[] slotTemporarilyEmpty = new bool[4]; // Geçici olarak boş slotları takip eder

    public bool AddItem(InventoryItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null && !slotTemporarilyEmpty[i])
            {
                slots[i] = item;
                return true;
            }
        }
        return false; // Envanter dolu
    }
    public bool CanAddItem()
    {
        // Check if there's any empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                Debug.Log($"Slot {i} is empty. Can add item.");
                return true;
            }
        }
        Debug.Log("No empty slots available. Cannot add item.");
        return false;
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
                slotTemporarilyEmpty[i] = false;
                return;
            }
        }
    }

    // Belirli slottan eşyayı geçici olarak alma
    public InventoryItemData TakeItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return null;

        if (slots[slotIndex] == null || slotTemporarilyEmpty[slotIndex])
            return null;

        InventoryItemData item = slots[slotIndex];
        slotTemporarilyEmpty[slotIndex] = true; // Slot geçici olarak boş işaretlenir
        
        return item;
    }

    // Eşyayı belirli slota geri koyma
    public bool PutItemBackToSlot(InventoryItemData item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return false;

        if (!slotTemporarilyEmpty[slotIndex])
            return false;

        if (slots[slotIndex] != item)
            return false;

        slotTemporarilyEmpty[slotIndex] = false; // Slot artık dolu
        return true;
    }

    // Belirli slottaki eşyayı tamamen kaldırma (kullanıldığında)
    public void ConsumeItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        slots[slotIndex] = null;
        slotTemporarilyEmpty[slotIndex] = false;
    }

    // Slot durumunu kontrol etme
    public bool IsSlotAvailable(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return false;

        return slots[slotIndex] != null && !slotTemporarilyEmpty[slotIndex];
    }

    // Slot geçici olarak boş mu?
    public bool IsSlotTemporarilyEmpty(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return false;

        return slotTemporarilyEmpty[slotIndex];
    }

    // UI için slot durumunu al
    public InventoryItemData GetSlotItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return null;

        if (slotTemporarilyEmpty[slotIndex])
            return null;

        return slots[slotIndex];
    }
}
