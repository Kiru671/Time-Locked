using UnityEngine;

public class MirrorManager : MonoBehaviour
{
    public void TriggerItems(GameObject item, string groupId)
    {
        MirrorGroup[] mirrorGroups = FindObjectsOfType<MirrorGroup>();
        foreach (var group in mirrorGroups)
        {
            if (!group.ocuppied && groupId != group.groupId)
            {
                group.ShowItem(item);
            }
        }
    }
}
