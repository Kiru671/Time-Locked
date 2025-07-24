using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Mirror : NetworkBehaviour
{
    private MirrorManager mirrorManager;

    private void Start()
    {
        mirrorManager = FindAnyObjectByType<MirrorManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisplayServerRpc(ulong itemId)
    {
        NetworkObject originalItem = null;
        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemId, out originalItem);
        
        if (!found || originalItem == null)
        {
            return;
        }

        // Create the copy on server
        GameObject itemCopy = Instantiate(originalItem.gameObject, 
            transform.position - transform.up * 1.5f, 
            Quaternion.identity);
        
        itemCopy.GetComponent<NetworkObject>().Spawn();
        
        // Preserve the scale
        itemCopy.transform.localScale = originalItem.transform.localScale;
        
        // Enable collider
        Collider collider = itemCopy.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;
    }
    
    public void SendItem(ulong itemId)
    {
        Debug.LogWarning("Item sent");
        mirrorManager.TriggerItems(itemId, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}