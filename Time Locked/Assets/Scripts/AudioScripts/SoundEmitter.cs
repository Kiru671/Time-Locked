using System;
using UnityEngine;
using UnityEngine.Events;

namespace AudioScripts
{
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundData[] sdata;
        private Vector3 pos;
            
        public void PlaySound()
        {
            pos = transform.position;
            AudioManager.Instance.PlaySfx(sdata[0], pos);
        }
    }
}
