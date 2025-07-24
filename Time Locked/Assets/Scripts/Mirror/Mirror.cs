using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : NetworkBehaviour
{
    private MirrorManager mirrorManager;
    private Vector3 localUp;
    
    private void Start()
    {
        mirrorManager = FindAnyObjectByType<MirrorManager>();
        localUp = transform.TransformDirection(Vector3.up);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DisplayServerRpc(ulong itemId)
    {
        NetworkObject originalItem = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out originalItem);

        if (!found || originalItem == null) return;

        GameObject prefabToSpawn = FindMatchingPrefab(originalItem);
        if (prefabToSpawn == null) return;

        // Create the copy (don't parent it to mirror initially)
        GameObject itemCopy = Instantiate(prefabToSpawn);

        // Calculate the world position where we want the item to appear (in front of mirror)
        Vector3 spawnWorldPosition = transform.position + (-localUp * 1.5f);
    
        // Set the world position and rotation BEFORE spawning
        itemCopy.transform.position = spawnWorldPosition;
        itemCopy.transform.rotation = Quaternion.identity;
        itemCopy.transform.localScale = originalItem.transform.localScale * 10f;

        // Now spawn the NetworkObject
        NetworkObject itemNetObj = itemCopy.GetComponent<NetworkObject>();
        itemNetObj.Spawn();

        // Enable collider after spawning
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
        
        Debug.Log($"Spawned mirrored item at world position: {spawnWorldPosition}");
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