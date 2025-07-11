using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    [SerializeField, Range(1,25f)] private float pulseRecoveryTime = 1f;
    [SerializeField, Range(1,2f)] private float pulseSize = 0.25f;
    private Vector3 startScale;

    private void Start()
    {
        startScale = this.transform.localScale;
    }

    public void TriggerPulse()
    {
        transform.localScale = new Vector3(transform.localScale.x + pulseSize, 
            transform.localScale.y + pulseSize, transform.localScale.z + pulseSize);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, startScale, pulseRecoveryTime * Time.deltaTime);
    }
}
