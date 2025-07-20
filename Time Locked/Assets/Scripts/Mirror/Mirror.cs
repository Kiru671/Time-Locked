using System;
using Unity.Netcode;
using UnityEngine;

public class Mirror : NetworkBehaviour
{
    private MirrorManager mirrorManager;

    private void Start()
    {
        mirrorManager = FindAnyObjectByType<MirrorManager>();
    }

    [ClientRpc]
    public void DisplayClientRpc(ulong itemId)
    {
        NetworkObject item = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out item);

        NetworkObject sentItem = 
            Instantiate(item, transform.position - transform.up * 1.5f, Quaternion.identity, null);
        sentItem.transform.localScale = item.transform.localScale * 10f;
        sentItem.GetComponent<Collider>().enabled = true;
    }
    
    public void SendItem(ulong itemId)
    {
        Debug.LogWarning("Item sent");
        mirrorManager.TriggerItems(itemId, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}
