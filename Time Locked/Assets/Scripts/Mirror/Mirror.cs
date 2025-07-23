using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : NetworkBehaviour
{
    private MirrorManager mirrorManager;

    private void Start()
    {
        mirrorManager = FindAnyObjectByType<MirrorManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisplayServerRpc(ulong itemId)
    {
        Debug.Log("DisplayServerRpc called with itemId: " + itemId);
        
        NetworkObject originalItem = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out originalItem);
        
        if (!found || originalItem == null)
        {
            Debug.LogError("Item with ID " + itemId + " not found!");
            return;
        }

        Debug.Log("Found original item: " + originalItem.gameObject.name);

        // Create the copy on server
        GameObject itemCopy = Instantiate(originalItem.gameObject, 
            transform.position - transform.up * 1.5f, 
            Quaternion.identity);
        
        Debug.Log("Created copy: " + itemCopy.name);
        
        // Preserve the scale
        itemCopy.transform.localScale = originalItem.transform.localScale;
        
        // Enable collider
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
        
        // CRITICAL: Remove NetworkObject component from the copy
        /*
         * NetworkObject networkObject = itemCopy.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            Debug.Log("Removing NetworkObject component from copy");
            DestroyImmediate(networkObject);
        }
        else
        {
            Debug.Log("No NetworkObject component found on copy");
        }
         */
        
        // DO NOT CALL NetworkObject.Spawn() - this was causing the error
        Debug.Log("Copy created successfully without network spawning");
        
        // Sync the creation to all clients
        NotifyClientsOfNewItemClientRpc(
            itemId, 
            itemCopy.transform.position, 
            itemCopy.transform.rotation,
            itemCopy.transform.localScale*10
        );
    }
    
    [ClientRpc]
    private void NotifyClientsOfNewItemClientRpc(ulong originalItemId, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Debug.Log("NotifyClientsOfNewItemClientRpc called");
        
        // Skip if we're the server (already created)
        if (IsServer) 
        {
            Debug.Log("Skipping client creation on server");
            return;
        }
        
        NetworkObject originalItem = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(originalItemId, out originalItem);
        
        if (!found || originalItem == null) 
        {
            Debug.LogError("Original item not found on client");
            return;
        }
        
        Debug.Log("Creating copy on client");
        
        // Create copy on client
        GameObject itemCopy = Instantiate(originalItem.gameObject, position, rotation);
        itemCopy.transform.localScale = scale;
        
        // Remove NetworkObject component
        /*
         * NetworkObject networkObject = itemCopy.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            Debug.Log("Removing NetworkObject component from client copy");
            DestroyImmediate(networkObject);
        }
         */
        
        // Enable collider
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
            
        Debug.Log("Client copy created successfully");
    }
    
    public void SendItem(ulong itemId)
    {
        Debug.LogWarning("Item sent");
        mirrorManager.TriggerItems(itemId, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}