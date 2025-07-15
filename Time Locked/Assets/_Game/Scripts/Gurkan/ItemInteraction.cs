using UnityEngine;
using Unity.Netcode;

public class ItemInteraction : NetworkBehaviour, IInteractable
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

        // Tell the server to despawn this item so it disappears for everyone
        if (IsServer)
        {
            DespawnItem();
        }
        else
        {
            DespawnItemServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnItemServerRpc(ServerRpcParams rpcParams = default)
    {
        DespawnItem();
    }

    private void DespawnItem()
    {
        // Safely despawn the NetworkObject so all clients are updated
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
        else
        {
            // Fallback: destroy if not a networked object
            Destroy(gameObject);
        }
    }
}
