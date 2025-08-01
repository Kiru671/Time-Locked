using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MirrorGroup: NetworkBehaviour
{
    private List<Mirror> mirrorList;
    private Mirror[] mirrors;
    public string groupId;
    public bool ocuppied;

    void Start()
    {
        mirrors = gameObject.transform.GetComponentsInChildren<Mirror>();
        mirrorList = new List<Mirror>(mirrors);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ShowItemServerRPC(ulong itemId)
    {
        Debug.LogWarning("Showing items in group: " + groupId);
        foreach (var mirror in mirrors)
        {
            mirror.DisplayClientRpc(itemId);
        }
    }
}
