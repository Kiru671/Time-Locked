using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SceneNetworkObjectHandler : NetworkBehaviour
{
    [SerializeField] private string itemTag = "Pickupable"; // Optional: use tags to identify items
    [SerializeField] private bool autoSpawnAllNetworkObjects = true;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(SpawnSceneObjects());
        }
    }
    
    private IEnumerator SpawnSceneObjects()
    {
        // Wait a frame to ensure scene is fully loaded
        yield return null;
        
        Debug.Log("[SERVER] Starting to spawn in-scene NetworkObjects...");
        
        if (autoSpawnAllNetworkObjects)
        {
            // Find ALL NetworkObjects in the scene
            NetworkObject[] allNetworkObjects = FindObjectsOfType<NetworkObject>();
            
            foreach (NetworkObject netObj in allNetworkObjects)
            {
                // Skip if already spawned (like the player or this manager)
                if (!netObj.IsSpawned && netObj.gameObject.scene.name != null)
                {
                    // Make sure it's a scene object (not a prefab in project)
                    ItemInteraction itemInteraction = netObj.GetComponent<ItemInteraction>();
                    if (itemInteraction != null)
                    {
                        Debug.Log($"[SERVER] Spawning scene object: {netObj.name}");
                        netObj.Spawn();
                    }
                }
            }
        }
        else if (!string.IsNullOrEmpty(itemTag))
        {
            // Find only objects with specific tag
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(itemTag);
            
            foreach (GameObject obj in taggedObjects)
            {
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    Debug.Log($"[SERVER] Spawning tagged scene object: {netObj.name}");
                    netObj.Spawn();
                }
            }
        }
        
        Debug.Log("[SERVER] Finished spawning in-scene NetworkObjects");
    }
    
    // Debug method to check scene objects
    [ContextMenu("Debug Scene NetworkObjects")]
    private void DebugSceneNetworkObjects()
    {
        NetworkObject[] allNetworkObjects = FindObjectsOfType<NetworkObject>();
        
        Debug.Log($"=== SCENE NETWORK OBJECTS ({allNetworkObjects.Length} total) ===");
        foreach (NetworkObject netObj in allNetworkObjects)
        {
            string status = netObj.IsSpawned ? "SPAWNED" : "NOT SPAWNED";
            Debug.Log($"{netObj.name} - {status} - ID: {netObj.NetworkObjectId}");
        }
        Debug.Log("================================");
    }
}

// ===== SETUP INSTRUCTIONS =====
/*
1. Create an empty GameObject in your scene
2. Add NetworkObject component to it
3. Add this SceneNetworkObjectHandler component
4. Make sure this GameObject is at the top of the scene hierarchy
5. Either:
   - Set autoSpawnAllNetworkObjects = true to spawn all items automatically
   - Or tag your pickupable items with "Pickupable" and set the itemTag

The script will automatically spawn all in-scene placed items when the server starts.
*/