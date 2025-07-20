using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GamePlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    private int nextSpawnPointIndex = 0;
    private bool firstPlayerAssigned = false; // Tracks if the orange player has been spawned

    public override void OnNetworkSpawn()
    {
        Debug.Log("GamePlayerSpawner.OnNetworkSpawn has been called.");
        if (!IsServer)
        {
            Debug.Log("This instance is not a server, so it will not spawn players.");
            return;
        }
        
        Debug.Log("This is the server. Spawning players for all connected clients.");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }

        // Subscribe to spawn players for new clients that connect later
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe when this spawner is destroyed
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
        }
        base.OnNetworkDespawn();
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned to the GamePlayerSpawner! Spawning at default location.");
            // Fallback to the spawner's own transform if no points are set
            return transform; 
        }

        Transform selectedSpawnPoint = spawnPoints[nextSpawnPointIndex];
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Count;
        return selectedSpawnPoint;
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        Debug.Log($"Attempting to spawn player for client ID: {clientId}");
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var connectedClient) && connectedClient.PlayerObject != null)
        {
            Debug.Log($"Client {clientId} already has a player object. Skipping spawn.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned on the GamePlayerSpawner!");
            return;
        }

        Transform sp = GetNextSpawnPoint();
        GameObject playerInstance = Instantiate(playerPrefab, sp.position, sp.rotation);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, false);
        
        

        // Assign player color (orange for the first player, blue for all others)
        bool assignOrange = !firstPlayerAssigned;
        if (!firstPlayerAssigned) firstPlayerAssigned = true;
        var playerSetup = playerInstance.GetComponent<PlayerNetworkSetup>();
        if (playerSetup != null)
        {
            playerSetup.SetInitialColor(assignOrange);
        }
    }
} 