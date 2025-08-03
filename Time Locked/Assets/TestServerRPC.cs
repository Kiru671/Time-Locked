using Unity.Netcode;
using UnityEngine;

public class TestServerRpc : NetworkBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"[{(IsServer ? "SERVER" : "CLIENT")}] Sending test RPC...");
            TestRpcServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void TestRpcServerRpc()
    {
        Debug.Log("[SERVER] Test RPC received successfully!");
        TestRpcClientRpc();
    }
    
    [ClientRpc]
    void TestRpcClientRpc()
    {
        Debug.Log($"[CLIENT-{NetworkManager.Singleton.LocalClientId}] Client RPC received!");
    }
}