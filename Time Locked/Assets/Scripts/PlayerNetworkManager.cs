using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    private HeldItemManager heldItemManager;
    public override void OnNetworkSpawn()
    {
        heldItemManager = GetComponent<HeldItemManager>();
        if (!IsOwner)
        {
            heldItemManager.enabled = false;
            MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>()
                .Where(component => !(component is NetworkBehaviour))
                .ToArray();
            foreach (var component in components)
            {
                component.enabled = false;
            }
            return;
        }
        PlayerInventory inv = gameObject.AddComponent<PlayerInventory>();
        StartCoroutine(AssignUIWhenReady(inv));
    }
    private IEnumerator AssignUIWhenReady(PlayerInventory inv)
    {
        // Wait until InventoryUIController.Instance is assigned
        while (InventoryUIController.Instance == null)
            yield return null;

        InventoryUIController.Instance.AssignToInventory(inv);
    }
    
    void Awake()
    {
        heldItemManager = GetComponent<HeldItemManager>();
    }
    
    public void RequestPickupItem(NetworkObject item, Transform handTransform)
    {
        if (item == null || handTransform == null) return;
        
        NetworkObject handNetObj = handTransform.GetComponent<NetworkObject>();
        if (handNetObj == null) return;
        
        RequestReparentServerRpc(item.NetworkObjectId, handNetObj.NetworkObjectId);
    }
    
    [ServerRpc]
    void RequestReparentServerRpc(ulong itemId, ulong handId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject item)) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(handId, out NetworkObject hand)) return;
        
        // OwnerClientId here is the player who called this RPC
        item.ChangeOwnership(OwnerClientId);
        item.TrySetParent(hand.transform);
    }
}
