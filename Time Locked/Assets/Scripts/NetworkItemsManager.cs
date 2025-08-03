using Unity.Netcode;
using UnityEngine;

public class NetworkItemsManager : MonoBehaviour
{
    void Start()
    {
        // Wait a bit for network to be ready
        Invoke(nameof(SpawnSceneItems), 1f);
    }
    
    void SpawnSceneItems()
    {
        // Only Host spawns items
        if (!NetworkManager.Singleton.IsHost) return;
        
        // Find all items in scene
        ItemInteraction[] items = FindObjectsOfType<ItemInteraction>();
        
        foreach (var item in items)
        {
            NetworkObject netObj = item.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
                Debug.Log($"✅ Spawned scene item: {item.name}");
            }
        }
    }
}