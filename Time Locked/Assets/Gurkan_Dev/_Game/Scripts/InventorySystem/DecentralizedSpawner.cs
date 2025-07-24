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
        
        // 1) Register all manually-assigned spawnablePrefabs
        foreach (var p in spawnablePrefabs)
        {
            _prefabLookup[p.PrefabIdHash] = p;
        }

        // 2) Register every prefab from NetworkManager’s NetworkConfig
        //    (NetworkConfig.Prefabs is a NetworkPrefabs; .Prefabs is the list)
        var netPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        foreach (var netPrefab in netPrefabs)
        {
            // netPrefab.Prefab is your GameObject
            var go = netPrefab.Prefab;
            var no = go.GetComponent<NetworkObject>();
            if (no != null)
            {
                _prefabLookup[no.PrefabIdHash] = no;
            }
        }

        // 3) (Optional) Debug-dump what’s actually registered
        foreach (var kv in _prefabLookup)
        {
            Debug.Log($"Registered prefab hash {kv.Key} → {kv.Value.name}");
        }
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

   

        // 4) apply the requested localScale
        netObj.transform.localScale = localScale;

        // 5) spawn over the network
        netObj.Spawn();
        
        // inform clients to reparent
        if (parentId != 0)
            SetParentClientRpc(netObj.NetworkObjectId, parentId);
        
        if (parentId != 0 &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentId, out var parentNO))
        {
            if (!netObj.TrySetParent(parentNO, worldPositionStays: true))
                Debug.LogWarning($"Failed to parent {netObj.name} under {parentNO.name}");
        }

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
    [ClientRpc]
    private void SetParentClientRpc(ulong childId, ulong parentId, ClientRpcParams parms = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(childId, out var child)
            && NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(parentId, out var parent))
        {
            child.transform.SetParent(parent.transform, worldPositionStays: true);
        }
    }
}


