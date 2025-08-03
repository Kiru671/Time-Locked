using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPickupable : NetworkBehaviour
{
    [Header("Pickup Settings")]
    public bool canBePickedUp = true;
    public float pickupRange = 3f;
    
    private NetworkVariable<bool> isBeingHeld = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> holderClientId = new NetworkVariable<ulong>();
    
    private Rigidbody rb;
    private Collider col;
    private Vector3 originalScale;
    
    public bool IsBeingHeld => isBeingHeld.Value;
    public ulong HolderClientId => holderClientId.Value;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalScale = transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Subscribe to network variable changes
        isBeingHeld.OnValueChanged += OnHeldStateChanged;
        holderClientId.OnValueChanged += OnHolderChanged;
    }

    public override void OnNetworkDespawn()
    {
        isBeingHeld.OnValueChanged -= OnHeldStateChanged;
        holderClientId.OnValueChanged -= OnHolderChanged;
        base.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ulong requestingClientId)
    {
        if (!canBePickedUp || isBeingHeld.Value)
        {
            Debug.Log($"Cannot pickup {name}: canBePickedUp={canBePickedUp}, isBeingHeld={isBeingHeld.Value}");
            return;
        }

        // Change ownership and mark as held
        NetworkObject.ChangeOwnership(requestingClientId);
        isBeingHeld.Value = true;
        holderClientId.Value = requestingClientId;
        
        Debug.Log($"Item {name} picked up by client {requestingClientId}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestReleaseServerRpc()
    {
        if (!isBeingHeld.Value) return;

        isBeingHeld.Value = false;
        holderClientId.Value = 0;
        
        Debug.Log($"Item {name} released");
    }

    private void OnHeldStateChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // Item was picked up
            if (rb != null) rb.isKinematic = true;
            if (col != null) col.enabled = false;
        }
        else
        {
            // Item was released
            if (rb != null) rb.isKinematic = false;
            if (col != null) col.enabled = true;
            transform.localScale = originalScale;
        }
    }

    private void OnHolderChanged(ulong previousValue, ulong newValue)
    {
        Debug.Log($"Item {name} holder changed from {previousValue} to {newValue}");
    }

    // Helper method to check if this client can pick up the item
    public bool CanClientPickup(ulong clientId)
    {
        return canBePickedUp && !isBeingHeld.Value;
    }
}