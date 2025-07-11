using System.Collections;
using System.Collections.Generic;
using AudioScripts;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] private SoundData[] sdata;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AudioManager.Instance.PlayRandom(sdata, transform.position);
        }
    }
}
