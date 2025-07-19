using System;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    private MirrorManager mirrorManager;

    private void Start()
    {
        mirrorManager = FindAnyObjectByType<MirrorManager>();
    }

    public void Display(GameObject item)
    {
        GameObject sentItem = 
            Instantiate(item, transform.position - transform.up * 1.5f, Quaternion.identity, null);
        sentItem.transform.localScale = item.transform.localScale * 10f;
        sentItem.GetComponent<Collider>().enabled = true;
    }

    public void SendItem(GameObject item)
    {
        if(item == null)
            return;
        mirrorManager.TriggerItems(item, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}
