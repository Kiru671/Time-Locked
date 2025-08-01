using UnityEngine;
using Unity.Netcode;

public class CubeSpawner : NetworkBehaviour, IInteractable
{
    public GameObject cubePrefab;

    public string GetInteractText()
    {
        return "Press 'E' to spawn a cube";
    }

    public void Interact()
    {
        SpawnCubeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCubeServerRpc(ServerRpcParams rpcParams = default)
    {
        if (cubePrefab != null)
        {
            GameObject cube = Instantiate(cubePrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            cube.GetComponent<NetworkObject>().Spawn();
        }
    }

    public string GetInteractionText()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerInventory player)
    {
        throw new System.NotImplementedException();
    }
} 