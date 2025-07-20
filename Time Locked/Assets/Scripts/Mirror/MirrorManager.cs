using Unity.Netcode;
using UnityEngine;

public class MirrorManager : NetworkBehaviour
{
    public void TriggerItems(ulong itemId, string groupId)
    {
        Debug.LogWarning("Triggering items for group: " + groupId + " with itemId: " + itemId);
        MirrorGroup[] mirrorGroups = FindObjectsOfType<MirrorGroup>();
        foreach (var group in mirrorGroups)
        {
            if (!group.ocuppied && groupId != group.groupId)
            {
                Debug.LogWarning("Showing item in group: " + group.groupId);
                group.ShowItemServerRPC(itemId);
            }
        }
    }
}
