using UnityEngine;
using Unity.Netcode;
using System;

[Serializable]
public struct PickupRequestData : INetworkSerializable
{
    public ulong clientId;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
    }
}

public class ItemInteraction : NetworkBehaviour, IInteractable
{
    [Header("Item Settings")]
    public InventoryItemData itemData;
    public string interactionText = "Press E to take";
    
    [Header("Pickup Settings")]
    public bool removeColliderOnPickup = true;
    public bool removeRigidbodyOnPickup = false;

    // Network variables
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<ulong> pickedUpBy = new NetworkVariable<ulong>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Cache components
    private Collider itemCollider;
    private Rigidbody itemRigidbody;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
    }
    
    [ContextMenu("Debug Network State")]
    private void DebugNetworkState()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        Debug.Log($"=== ITEM DEBUG: {gameObject.name} ===");
        Debug.Log($"Has NetworkObject: {netObj != null}");
        Debug.Log($"IsSpawned: {(netObj != null ? netObj.IsSpawned : false)}");
        Debug.Log($"NetworkObjectId: {(netObj != null ? netObj.NetworkObjectId : 0)}");
        Debug.Log($"IsServer: {IsServer}");
        Debug.Log($"IsClient: {IsClient}");
        Debug.Log($"IsOwner: {(netObj != null ? netObj.IsOwner : false)}");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[{(IsServer ? "SERVER" : "CLIENT")}] ItemInteraction NetworkSpawn for {itemData?.itemName ?? "Unknown"}");
        Debug.Log($"[{(IsServer ? "SERVER" : "CLIENT")}] NetworkObjectId: {NetworkObjectId}");
        Debug.Log($"[{(IsServer ? "SERVER" : "CLIENT")}] IsSpawned: {IsSpawned}");
    
        if (IsServer)
        {
            isPickedUp.Value = false;
            pickedUpBy.Value = 0;
        }
    
        isPickedUp.OnValueChanged += OnPickupStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        isPickedUp.OnValueChanged -= OnPickupStateChanged;
    }

    public string GetInteractionText()
    {
        return isPickedUp.Value ? "" : $"{interactionText} '{itemData.itemName}'";
    }

    public void Interact(PlayerInventory player)
    {
        Debug.Log($"[CLIENT] Interact called for {itemData?.itemName}");
        
        if (player == null || isPickedUp.Value || !player.inventorySystem.CanAddItem())
        {
            Debug.Log("Cannot pickup - invalid state");
            return;
        }

        // Send pickup request
        SendPickupRequest();
    }

    private void SendPickupRequest()
    {
        Debug.Log($"[CLIENT] ===== SENDING PICKUP REQUEST =====");
        Debug.Log($"[CLIENT] IsSpawned: {IsSpawned}");
        Debug.Log($"[CLIENT] NetworkManager connected: {NetworkManager.Singleton.IsConnectedClient}");
        Debug.Log($"[CLIENT] LocalClientId: {NetworkManager.Singleton.LocalClientId}");
        Debug.Log($"[CLIENT] IsServer: {IsServer}");
        Debug.Log($"[CLIENT] IsClient: {IsClient}");
    
        if (!IsSpawned)
        {
            Debug.LogError("Item not spawned!");
            return;
        }

        Debug.Log($"[CLIENT] Sending pickup request to server");
    
        // Create request data
        var requestData = new PickupRequestData { clientId = NetworkManager.Singleton.LocalClientId };
    
        // Call the RPC
        HandlePickupRequestServerRpc(requestData);
        Debug.Log($"[CLIENT] RPC call completed");
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandlePickupRequestServerRpc(PickupRequestData request)  // <-- Remove RpcParams
    {
        Debug.Log("[SERVER] GOT RPC FROM CLIENT!");
    
        // Get sender ID differently
        ulong actualClientId = NetworkManager.Singleton.LocalClientId; // This won't work for validation
    
        // For now, just trust the request
        ProcessPickupOnServer(request.clientId);  // Use the clientId from request
    }

    private void ProcessPickupOnServer(ulong clientId)
    {
        Debug.Log($"[SERVER] ===== PROCESSING PICKUP =====");
        Debug.Log($"[SERVER] Client ID: {clientId}");
        Debug.Log($"[SERVER] IsServer check: {IsServer}");
        
        if (!IsServer) 
        {
            Debug.LogError("[SERVER] ProcessPickupOnServer called but IsServer is false!");
            return;
        }
        
        Debug.Log($"[SERVER] Item already picked up: {isPickedUp.Value}");
        
        if (isPickedUp.Value)
        {
            Debug.Log("[SERVER] Item already picked up");
            NotifyPickupResultClientRpc(clientId, false, "Already picked up");
            return;
        }

        // Find player
        Debug.Log($"[SERVER] Looking for client {clientId} in connected clients...");
        
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
        {
            Debug.LogError($"[SERVER] Client {clientId} not found in connected clients!");
            
            // Debug all connected clients
            Debug.Log("[SERVER] Connected clients:");
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                Debug.Log($"[SERVER] - Client {kvp.Key}: {kvp.Value.PlayerObject?.name ?? "null"}");
            }
            return;
        }

        Debug.Log($"[SERVER] Found client data for {clientId}");
        
        var playerObj = clientData.PlayerObject;
        if (playerObj == null)
        {
            Debug.LogError($"[SERVER] Player object not found for client {clientId}");
            return;
        }

        Debug.Log($"[SERVER] Found player object: {playerObj.name}");

        var playerInventory = playerObj.GetComponentInChildren<PlayerInventory>();
        if (playerInventory == null)
        {
            Debug.LogError("[SERVER] PlayerInventory component not found!");
            
            // Try to find it differently
            playerInventory = playerObj.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogError("[SERVER] PlayerInventory not found with GetComponent either!");
                return;
            }
        }

        Debug.Log($"[SERVER] Found PlayerInventory");
        Debug.Log($"[SERVER] Inventory system null check: {playerInventory.inventorySystem == null}");
        
        if (playerInventory.inventorySystem == null)
        {
            Debug.LogError("[SERVER] InventorySystem is null!");
            NotifyPickupResultClientRpc(clientId, false, "Inventory system error");
            return;
        }

        bool canAdd = playerInventory.inventorySystem.CanAddItem();
        Debug.Log($"[SERVER] CanAddItem result: {canAdd}");

        if (!canAdd)
        {
            Debug.Log("[SERVER] Inventory full or cannot add item");
            NotifyPickupResultClientRpc(clientId, false, "Inventory full");
            return;
        }

        // Success - update state
        Debug.Log("[SERVER] SUCCESS - Updating pickup state");
        isPickedUp.Value = true;
        pickedUpBy.Value = clientId;
        
        // Change ownership
        GetComponent<NetworkObject>().ChangeOwnership(clientId);
        
        // Notify client of success
        NotifyPickupResultClientRpc(clientId, true, "");
        Debug.Log("[SERVER] Pickup processing complete");
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyPickupResultClientRpc(ulong targetClientId, bool success, string reason)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        Debug.Log($"[CLIENT] Pickup result: {(success ? "Success" : $"Failed - {reason}")}");
        
        if (success)
        {
            // Add to local inventory
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            var inventory = localPlayer.GetComponentInChildren<PlayerInventory>();
            
            if (inventory != null)
            {
                inventory.TryAddItemWithWorldObject(itemData, gameObject);
            }
        }
    }

    private void OnPickupStateChanged(bool previous, bool current)
    {
        Debug.Log($"[{(IsServer ? "SERVER" : "CLIENT")}] Pickup state changed: {previous} -> {current}");
        
        if (current)
        {
            // Picked up
            if (itemCollider != null) itemCollider.enabled = false;
            if (itemRigidbody != null) itemRigidbody.isKinematic = true;
            gameObject.SetActive(false);
        }
        else
        {
            // Released
            if (itemCollider != null) itemCollider.enabled = true;
            if (itemRigidbody != null) itemRigidbody.isKinematic = false;
            gameObject.SetActive(true);
        }
    }

    // Public properties
    public bool IsPickedUp => isPickedUp.Value;
    public ulong PickedUpByClientId => pickedUpBy.Value;
}