using UnityEngine;
using UnityEngine.Events;

public class ItemUsageZone : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    public string requiredItemName;
    
    [Header("Usage Settings")]
    public bool requireHeldItem = true; // Eşyanın elimizde olması gerekli mi?
    public bool consumeItem = true; // Eşya kullanıldığında tüketilsin mi?
    
    [Header("Events")]
    public UnityEvent onUse;
    public UnityEvent onWrongItem; // Yanlış eşya tuttuğunda
    public UnityEvent onNoHeldItem; // Elimizde eşya yokken

    public string GetInteractionText()
    {
        if (string.IsNullOrEmpty(requiredItemName))
        {
            return "Press E to interact";
        }

        if (requireHeldItem)
        {
            return $"Press E to use '{requiredItemName}' (hold in hand)";
        }
        else
        {
            return $"Press E to use '{requiredItemName}'";
        }
    }

    public void Interact(PlayerInventory player)
    {
        if (string.IsNullOrEmpty(requiredItemName))
        {
            // Eşya gerektirmiyorsa direkt kullan
            onUse.Invoke();
            Debug.Log("✅ Used interaction zone");
            return;
        }

        if (requireHeldItem)
        {
            // Elimizde tutulan eşyayı kontrol et
            if (!player.IsHoldingItem())
            {
                Debug.Log($"❌ No item in hand! Need to hold '{requiredItemName}'");
                onNoHeldItem.Invoke();
                return;
            }

            InventoryItemData heldItem = player.GetHeldItem();
            if (heldItem.itemName != requiredItemName)
            {
                Debug.Log($"❌ Wrong item in hand! Need '{requiredItemName}' but holding '{heldItem.itemName}'");
                onWrongItem.Invoke();
                return;
            }

            // Doğru eşya elimizde - kullan
            if (consumeItem)
            {
                player.UseHeldItem(); // Eşyayı tüket
                Debug.Log($"✅ Used '{requiredItemName}' from hand!");
            }
            else
            {
                Debug.Log($"✅ Used '{requiredItemName}' (not consumed)");
            }
            
            onUse.Invoke();
        }
        else
        {
            // Eski sistem - sadece envanterda olması yeterli
            if (player.HasItem(requiredItemName))
            {
                if (consumeItem)
                {
                    player.TryRemoveItem(requiredItemName);
                }
                onUse.Invoke();
                Debug.Log($"✅ Used '{requiredItemName}' from inventory!");
            }
            else
            {
                Debug.Log($"❌ You don't have '{requiredItemName}' in inventory!");
            }
        }
    }

    // Zone'a girdiğinde bilgi göster
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory player = other.GetComponent<PlayerInventory>();
            if (player != null && !string.IsNullOrEmpty(requiredItemName))
            {
                if (requireHeldItem)
                {
                    if (player.IsHoldingItem())
                    {
                        InventoryItemData heldItem = player.GetHeldItem();
                        if (heldItem.itemName == requiredItemName)
                        {
                            Debug.Log($"💡 You can use the {requiredItemName} here!");
                        }
                        else
                        {
                            Debug.Log($"💡 This zone needs '{requiredItemName}', but you're holding '{heldItem.itemName}'");
                        }
                    }
                    else
                    {
                        Debug.Log($"💡 Hold '{requiredItemName}' to use this zone");
                    }
                }
            }
        }
    }
}
