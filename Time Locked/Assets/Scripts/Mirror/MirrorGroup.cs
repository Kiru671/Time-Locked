using System.Collections.Generic;
using UnityEngine;

public class MirrorGroup: MonoBehaviour
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
    
    public void ShowItem(GameObject item)
    {
        foreach (var mirror in mirrors)
        {
            mirror.Display(item);
        }
    }
}
