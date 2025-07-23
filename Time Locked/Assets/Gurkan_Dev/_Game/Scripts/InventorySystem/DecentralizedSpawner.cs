using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum SpawnAudience : byte { Everyone, OnlyTheseClients }

public class DecentralizedSpawner : NetworkBehaviour
{
    public static DecentralizedSpawner Instance { get; private set; }

    [SerializeField] private NetworkObject[] spawnablePrefabs;
    private Dictionary<uint, NetworkObject> _prefabLookup;

    void Awake()
    {
        Instance = this;
        _prefabLookup = spawnablePrefabs.ToDictionary(p => p.PrefabIdHash);
    }

    /// <summary>Call from ANY client to ask the server to spawn with a custom localScale.</summary>
    public void RequestSpawn(NetworkObject prefab,
                             Vector3 position,
                             Quaternion rotation,
                             Vector3 localScale,
                             NetworkObject parent = null,
                             IReadOnlyList<ulong> onlyForClients = null)
    {
        uint prefabHash = prefab.PrefabIdHash;
        ulong parentId = parent ? parent.NetworkObjectId : 0;
        ulong[] targets = onlyForClients?.ToArray() ?? Array.Empty<ulong>();

        SpawnRequestServerRpc(prefabHash, position, rotation, localScale, parentId, targets);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnRequestServerRpc(uint prefabHash,
                                       Vector3 position,
                                       Quaternion rotation,
                                       Vector3 localScale,
                                       ulong parentId,
                                       ulong[] targetClientIds,
                                       ServerRpcParams rpcParams = default)
    {
        // 1) lookup
        if (!_prefabLookup.TryGetValue(prefabHash, out var prefab))
        {
            Debug.LogWarning($"Prefab hash {prefabHash} not registered.");
            return;
        }

        // 2) instantiate at world‐space pos/rot
        var netObj = Instantiate(prefab, position, rotation);

        // 3) optional reparent
        if (parentId != 0 &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentId, out var parentNO))
        {
            netObj.TrySetParent(parentNO, worldPositionStays: true);
        }

        // 4) apply the requested localScale
        netObj.transform.localScale = localScale;

        // 5) spawn over the network
        netObj.Spawn();

#if UNITY_NETCODE_1_4_OR_NEWER
        // 6) optionally hide for clients not in the target list
        if (targetClientIds.Length > 0)
        {
            foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (id == NetworkManager.ServerClientId) continue;
                if (!targetClientIds.Contains(id))
                    netObj.NetworkHide(id);
            }
        }
#endif
    }
}
