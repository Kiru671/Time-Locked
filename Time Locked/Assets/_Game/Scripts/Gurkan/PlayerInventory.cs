using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Components")]
    public InventorySystem inventorySystem;
    public InventoryUIController uiController;
    public HeldItemManager heldItemManager;
    public ItemInspector itemInspector; // Inspect sistemi

    [Header("Input Settings")]
    [SerializeField] private KeyCode[] slotKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    [SerializeField] public bool enableRightClickCancel = true; // Sağ tık ile iptal etmeyi aktif et
    [SerializeField] private KeyCode inspectKey = KeyCode.Tab; // Envanterdeki eşyayı inspect etme tuşu

    // Her slot için dünya objesini sakla
    private Dictionary<int, GameObject> slotWorldObjects = new Dictionary<int, GameObject>();

    private void Start()
    {
        uiController = FindObjectOfType<InventoryUIController>();
        
        // ItemInspector'ı otomatik bul
        if (itemInspector == null)
        {
            itemInspector = FindObjectOfType<ItemInspector>();
            if (itemInspector == null)
            {
                Debug.LogWarning("ItemInspector not found! Tab inspect feature won't work.");
            }
        }

        uiController.RefreshUI(inventorySystem, heldItemManager);
        ShowInitialInstructions();
    }

    private void Update()
    {
        HandleSlotInput();
        HandleCancelInput();
        HandleInspectInput();
    }

    private void ShowInitialInstructions()
    {
        Debug.Log("🎮 ENHANCED INVENTORY CONTROLS:");
        Debug.Log("• 1,2,3,4: Pick up/Put back items");
        Debug.Log("• E: Pick up items from world");
        Debug.Log("• F: Inspect items in world (without pickup)");
        Debug.Log("• Tab: Inspect held item from inventory");
        Debug.Log("• Right Click: Cancel carrying (put back item)");
        Debug.Log("• Items will appear physically in your hand!");
        Debug.Log("========================================");
    }

    private void HandleSlotInput()
    {
        for (int i = 0; i < slotKeys.Length; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                HandleSlotInteraction(i);
            }
        }
    }

    private void HandleCancelInput()
    {
        // Sağ tık veya Esc ile taşımayı iptal et
        if ((enableRightClickCancel && Input.GetMouseButtonDown(1)) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Inspect modundayken farklı davranış
            if (itemInspector != null && itemInspector.IsInspecting)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log("💭 Cannot cancel carrying while inspecting! Press Esc or Tab first.");
                    return;
                }
                // Esc tuşuysa ItemInspector halledecek
                return;
            }
            
            // Sadece taşıyorsak iptal et
            if (heldItemManager.IsHoldingItem)
            {
                CancelCarrying();
            }
        }
    }

    private void HandleInspectInput()
    {
        // Tab tuşu ile elimizdeki eşyayı inspect et
        if (Input.GetKeyDown(inspectKey))
        {
            InspectHeldItem();
        }
    }

    private void HandleSlotInteraction(int slotIndex)
    {
        // Eğer elimizde bir eşya varsa
        if (heldItemManager.IsHoldingItem)
        {
            // Eşyayı geri koy
            if (heldItemManager.HeldItemSlotIndex == slotIndex)
            {
                PutBackHeldItem();
            }
            // Farklı bir slota eşya koymaya çalışıyorsa, şimdilik sadece mevcut eşyayı geri koy
            else
            {
                PutBackHeldItem();
                // Animasyon bittikten sonra yeni eşyayı al
                StartCoroutine(DelayedTakeItem(slotIndex));
            }
        }
        else
        {
            // Yeni eşya al
            TakeItemFromSlot(slotIndex);
        }
    }

    private System.Collections.IEnumerator DelayedTakeItem(int slotIndex)
    {
        yield return new WaitForSeconds(0.1f); // Kısa bir gecikme
        TakeItemFromSlot(slotIndex);
    }

    public void TakeItemFromSlot(int slotIndex)
    {
        if (!inventorySystem.IsSlotAvailable(slotIndex))
        {
            Debug.Log($"Slot {slotIndex + 1} is empty or unavailable");
            return;
        }

        InventoryItemData item = inventorySystem.TakeItemFromSlot(slotIndex);
        if (item != null)
        {
            // Dünya objesini al
            GameObject worldObject = null;
            if (slotWorldObjects.ContainsKey(slotIndex))
            {
                worldObject = slotWorldObjects[slotIndex];
            }

            if (heldItemManager.TakeItem(item, slotIndex, worldObject))
            {
                Debug.Log($"📦 Picked up {item.itemName} from slot {slotIndex + 1}");
                Debug.Log($"💡 Use at interaction zones, press {slotIndex + 1} to put back, or Right-Click to cancel");
                
                uiController.RefreshUI(inventorySystem, heldItemManager);
                
                // Pulse efekti ekle
                uiController.PulseHeldSlot();
            }
            else
            {
                // Eşyayı alınamazsa geri koy
                inventorySystem.PutItemBackToSlot(item, slotIndex);
                Debug.Log("❌ Could not take item - hands are full");
            }
        }
    }

    public void PutBackHeldItem()
    {
        if (!heldItemManager.IsHoldingItem)
            return;

        InventoryItemData heldItem = heldItemManager.CurrentHeldItem;
        int originalSlot = heldItemManager.HeldItemSlotIndex;

        heldItemManager.PutBackItem();

        if (heldItem != null)
        {
            inventorySystem.PutItemBackToSlot(heldItem, originalSlot);
            Debug.Log($"📥 Put back {heldItem.itemName} to slot {originalSlot + 1}");
            uiController.RefreshUI(inventorySystem, heldItemManager);
        }
    }

    // Sağ tık ile taşımayı iptal et
    private void CancelCarrying()
    {
        if (!heldItemManager.IsHoldingItem)
        {
            Debug.Log("💭 No item to cancel");
            return;
        }

        InventoryItemData cancelledItem = heldItemManager.CurrentHeldItem;
        Debug.Log($"🚫 Cancelled carrying {cancelledItem.itemName}");
        
        PutBackHeldItem();
        
        // İptal efekti için ekstra pulse
        uiController.PulseHeldSlot();
    }

    // Dünya objesi ile eşya ekleme (ItemInteraction'dan çağrılır)
    public bool TryAddItemWithWorldObject(InventoryItemData item, GameObject worldObject)
    {
        if (inventorySystem.AddItem(item))
        {
            // Hangi slota eklediğini bul
            int slotIndex = -1;
            for (int i = 0; i < inventorySystem.slots.Length; i++)
            {
                if (inventorySystem.slots[i] == item)
                {
                    slotIndex = i;
                    break;
                }
            }

            // Dünya objesini slot ile eşleştir
            if (slotIndex >= 0)
            {
                slotWorldObjects[slotIndex] = worldObject;
                Debug.Log($"✅ Added {item.itemName} to slot {slotIndex + 1} with world object");
            }

            uiController.RefreshUI(inventorySystem, heldItemManager);
            return true;
        }
        else
        {
            Debug.Log($"❌ Inventory full! Cannot add {item.itemName}");
            return false;
        }
    }

    public void TryAddItem(InventoryItemData item)
    {
        if (inventorySystem.AddItem(item))
        {
            Debug.Log($"✅ Added {item.itemName} to inventory");
            uiController.RefreshUI(inventorySystem, heldItemManager);
        }
        else
        {
            Debug.Log($"❌ Inventory full! Cannot add {item.itemName}");
        }
    }

    public void TryRemoveItem(string itemName)
    {
        // Önce hangi slottan çıkaracağını bul
        int slotIndex = -1;
        for (int i = 0; i < inventorySystem.slots.Length; i++)
        {
            if (inventorySystem.slots[i] != null && inventorySystem.slots[i].itemName == itemName)
            {
                slotIndex = i;
                break;
            }
        }

        inventorySystem.RemoveItem(itemName);
        
        // Dünya objesini de temizle
        if (slotIndex >= 0 && slotWorldObjects.ContainsKey(slotIndex))
        {
            slotWorldObjects.Remove(slotIndex);
        }
        
        Debug.Log($"🗑️ Removed {itemName} from inventory");
        uiController.RefreshUI(inventorySystem, heldItemManager);
    }

    public bool HasItem(string itemName) => inventorySystem.HasItem(itemName);

    // ItemUsageZone'dan çağrılacak eşya kullanım metodu
    public void UseHeldItem()
    {
        if (!heldItemManager.IsHoldingItem)
        {
            Debug.Log("❌ No item in hand to use");
            return;
        }

        InventoryItemData usedItem = heldItemManager.CurrentHeldItem;
        int slotIndex = heldItemManager.HeldItemSlotIndex;

        // Dünya objesini tamamen yok et (kullanıldığında)
        if (slotWorldObjects.ContainsKey(slotIndex))
        {
            GameObject worldObj = slotWorldObjects[slotIndex];
            if (worldObj != null)
            {
                Destroy(worldObj);
            }
            slotWorldObjects.Remove(slotIndex);
        }

        heldItemManager.UseHeldItem();
        inventorySystem.ConsumeItemFromSlot(slotIndex);
        
        Debug.Log($"✨ Used {usedItem.itemName} at interaction zone!");
        uiController.RefreshUI(inventorySystem, heldItemManager);
    }

    // Elimizdeki eşyayı alma (başka scriptler için)
    public InventoryItemData GetHeldItem()
    {
        return heldItemManager.CurrentHeldItem;
    }

    // Elimizde eşya var mı kontrolü
    public bool IsHoldingItem()
    {
        return heldItemManager.IsHoldingItem;
    }

    // Elimizdeki dünya objesini alma
    public GameObject GetHeldWorldObject()
    {
        return heldItemManager.GetHeldWorldObject();
    }

    // Sağ tık özelliğini açma/kapama
    public void SetRightClickCancelEnabled(bool enabled)
    {
        enableRightClickCancel = enabled;
        Debug.Log($"Right-click cancel: {(enabled ? "Enabled" : "Disabled")}");
    }

    // Debug için envanter durumunu yazdırma
    [ContextMenu("Print Inventory Status")]
    public void PrintInventoryStatus()
    {
        Debug.Log("=== 📋 INVENTORY STATUS ===");
        for (int i = 0; i < inventorySystem.slots.Length; i++)
        {
            string status = "Empty";
            string icon = "🔲";
            
            if (inventorySystem.slots[i] != null)
            {
                status = inventorySystem.slots[i].itemName;
                icon = "📦";
                
                if (slotWorldObjects.ContainsKey(i))
                {
                    status += $" (WorldObj: {slotWorldObjects[i].name})";
                }
                
                if (inventorySystem.IsSlotTemporarilyEmpty(i))
                {
                    status += " (CARRIED)";
                    icon = "👜";
                }
            }
            Debug.Log($"{icon} Slot {i + 1}: {status}");
        }
        
        if (heldItemManager.IsHoldingItem)
        {
            Debug.Log($"✋ HELD ITEM: {heldItemManager.CurrentHeldItem.itemName} (from slot {heldItemManager.HeldItemSlotIndex + 1})");
            GameObject heldObj = heldItemManager.GetHeldWorldObject();
            if (heldObj != null)
            {
                Debug.Log($"✋ HELD WORLD OBJECT: {heldObj.name}");
            }
        }
        else
        {
            Debug.Log("✋ HELD ITEM: None");
        }
        Debug.Log("==========================");
    }

    // Elimizdeki eşyayı inspect etme
    private void InspectHeldItem()
    {
        if (itemInspector == null)
        {
            Debug.LogWarning("❌ ItemInspector not found! Cannot inspect item.");
            return;
        }

        if (itemInspector.IsInspecting)
        {
            Debug.Log("💭 Already inspecting an item!");
            return;
        }

        if (!heldItemManager.IsHoldingItem)
        {
            Debug.Log("❌ No item in hand to inspect!");
            return;
        }

        GameObject heldWorldObject = heldItemManager.GetHeldWorldObject();
        if (heldWorldObject == null)
        {
            Debug.Log("❌ Held item has no world object to inspect!");
            return;
        }

        // Inspectable component kontrolü
        Inspectable inspectable = heldWorldObject.GetComponent<Inspectable>();
        if (inspectable == null)
        {
            Debug.Log($"❌ {heldItemManager.CurrentHeldItem.itemName} is not inspectable!");
            return;
        }

        Debug.Log($"🎯 Starting inspection of held item: {heldItemManager.CurrentHeldItem.itemName}");
        itemInspector.InspectItem(heldWorldObject);
    }
}
