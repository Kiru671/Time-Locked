using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ServerObjectSpawner : NetworkBehaviour
{
    public static ServerObjectSpawner Instance { get; private set; }
    // Track unspawned objects by custom ID
    private Dictionary<ulong, NetworkObject> unspawnedObjects = new Dictionary<ulong, NetworkObject>();
    private ulong nextObjectId = 1;

    private void Awake()
    {
        Instance = this;
    }

    // Call this when creating objects you want to spawn later
    public ulong RegisterUnspawnedObject(NetworkObject networkObject)
    {
        ulong customId = nextObjectId++;
        unspawnedObjects[customId] = networkObject;
        
        // Make sure the object is not spawned yet
        if (networkObject.IsSpawned)
        {
            networkObject.Despawn(false); // Keep the GameObject
        }
        
        return customId;
    }

    private NetworkObject GetUnspawnedNetworkObject(ulong objId)
    {
        unspawnedObjects.TryGetValue(objId, out NetworkObject obj);
        return obj;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(ulong objId)
    {
        NetworkObject networkObject = GetUnspawnedNetworkObject(objId);
        
        if (networkObject != null && !networkObject.IsSpawned)
        {
            networkObject.Spawn();
            unspawnedObjects.Remove(objId); // Remove from unspawned tracking
        }
    }
    public void SpawnRegisteredObject(ulong objId)
    {
        if (IsServer)
        {
            NetworkObject networkObject = GetUnspawnedNetworkObject(objId);
            
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn();
                unspawnedObjects.Remove(objId);
                Debug.Log($"Spawned object with ID: {objId}");
            }
        }
    }
}