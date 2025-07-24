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
    
        if (!found || originalItem == null) return;

        GameObject prefabToSpawn = FindMatchingPrefab(originalItem);
        if (prefabToSpawn == null) return;

        // Create as child of mirror
        GameObject itemCopy = Instantiate(prefabToSpawn, transform);
    
        // Set local position relative to mirror
        itemCopy.transform.localPosition = -Vector3.up * 1.5f;
        itemCopy.transform.localRotation = Quaternion.identity;
        itemCopy.transform.localScale = originalItem.transform.lossyScale / transform.lossyScale.x;
    
        // Get the world position before spawning
        Vector3 worldPos = itemCopy.transform.position;
        Vector3 worldScale = itemCopy.transform.lossyScale;
    
        // Unparent before spawning
        itemCopy.transform.SetParent(null);
        itemCopy.transform.position = worldPos;
        itemCopy.transform.localScale = worldScale;
    
        // Now spawn
        NetworkObject itemNetObj = itemCopy.GetComponent<NetworkObject>();
        itemNetObj.Spawn();
    
        // Enable collider
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
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