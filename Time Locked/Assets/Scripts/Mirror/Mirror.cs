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
        NetworkObject originalItem = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out originalItem);
        
        if (!found || originalItem == null)
        {
            return;
        }

        // Debug logging
        Debug.Log($"Looking for prefab for: {originalItem.name} with hash: {originalItem.PrefabIdHash}");

        // Find the matching prefab from NetworkManager's list
        GameObject prefabToSpawn = FindMatchingPrefab(originalItem);
        if (prefabToSpawn == null)
        {
            Debug.LogError($"Could not find prefab for {originalItem.name} in NetworkManager's prefab list!");
            return;
        }

        // Create the copy from the registered prefab
        GameObject itemCopy = Instantiate(prefabToSpawn, 
            transform.position - transform.up * 1.5f, 
            Quaternion.identity);
        
        // Preserve the scale from the original
        itemCopy.transform.localScale = originalItem.transform.localScale;
        
        // Enable collider
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
            
        // Now spawn - this will work because we used a registered prefab
        itemCopy.GetComponent<NetworkObject>().Spawn();
    }
    
    private GameObject FindMatchingPrefab(NetworkObject spawnedInstance)
    {
        // Try multiple methods to find the prefab
        
        // Method 1: Direct hash comparison
        ulong instancePrefabHash = spawnedInstance.PrefabIdHash;
        Debug.Log($"Instance hash: {instancePrefabHash}");
        
        foreach (var registeredPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (registeredPrefab.Prefab != null)
            {
                NetworkObject prefabNetObj = registeredPrefab.Prefab.GetComponent<NetworkObject>();
                if (prefabNetObj != null)
                {
                    Debug.Log($"Checking prefab: {registeredPrefab.Prefab.name} with hash: {prefabNetObj.PrefabIdHash}");
                    
                    if (prefabNetObj.PrefabIdHash == instancePrefabHash)
                    {
                        Debug.Log($"Found match by hash!");
                        return registeredPrefab.Prefab;
                    }
                }
            }
        }
        
        // Method 2: Name matching (fallback)
        string cleanName = spawnedInstance.name.Replace("(Clone)", "").Trim();
        Debug.Log($"Trying name match for: {cleanName}");
        
        foreach (var registeredPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (registeredPrefab.Prefab != null && registeredPrefab.Prefab.name == cleanName)
            {
                Debug.Log($"Found match by name!");
                return registeredPrefab.Prefab;
            }
        }
        
        // Method 3: Try finding in spawned objects' prefab reference
        if (spawnedInstance.NetworkManager != null && spawnedInstance.NetworkManager.SpawnManager != null)
        {
            // Sometimes the prefab reference is stored internally
            var prefabHandler = spawnedInstance.NetworkManager.PrefabHandler;
            if (prefabHandler != null)
            {
                // This approach depends on your Netcode version
                Debug.Log("Trying through PrefabHandler...");
            }
        }
        
        return null;
    }
    
    public void SendItem(ulong itemId)
    {
        Debug.LogWarning("Item sent");
        mirrorManager.TriggerItems(itemId, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}