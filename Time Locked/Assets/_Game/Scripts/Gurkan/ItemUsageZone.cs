using UnityEngine;
using UnityEngine.Events;

public class ItemUsageZone : MonoBehaviour, IInteractable
{
    public string requiredItemName;
    public UnityEvent onUse;
    public bool recurrentUse = false;


    private bool _itemPlaced;

    public string GetInteractionText()
    {
        return $"Press E to use '{requiredItemName}'";
    }

    public void Interact(PlayerInventory player)
    {
        if (player.HasItem(requiredItemName))
        {
            player.TryRemoveItem(requiredItemName);
            onUse.Invoke();
            _itemPlaced = true;
        }
        if(_itemPlaced && recurrentUse)
            onUse.Invoke();
    }
}
