using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class QuickSpawnTest : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for network to initialize
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }
        
        // Only run on server
        if (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(1f); // Wait a bit for scene to settle
            
            Debug.Log("[SERVER] Spawning all in-scene items...");
            
            // Find all ItemInteraction components
            ItemInteraction[] items = FindObjectsOfType<ItemInteraction>();
            
            foreach (ItemInteraction item in items)
            {
                NetworkObject netObj = item.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                {
                    Debug.Log($"[SERVER] Spawning: {item.name}");
                    netObj.Spawn();
                }
            }
            
            Debug.Log($"[SERVER] Spawned {items.Length} items");
        }
    }
}