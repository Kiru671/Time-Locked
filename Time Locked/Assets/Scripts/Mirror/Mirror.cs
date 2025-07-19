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
        Instantiate(item, transform.position, transform.rotation, transform);
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject item = other.gameObject;
        if(item == null)
            return;
        mirrorManager.TriggerItems(item, transform.parent.GetComponent<MirrorGroup>().groupId);
    }
}
