using System;
using UnityEngine;
using UnityEngine.Events;

namespace AudioScripts
{
    public class BeatManager: MonoBehaviour
    {
        [SerializeField] private float bpm;
        public float BPM
        {
            get => bpm;
            set => bpm = value;
        }
        [HideInInspector] public AudioSource audioSource;
        [SerializeField] private Intervals[] intervals;

        private void Update()
        {
            foreach (var interval in intervals)
            {
                if(audioSource == null)
                    return;
                float sampledTime = (audioSource.timeSamples / (audioSource.clip.frequency * interval.GetIntervalLength(bpm)));
                interval.CheckForNewInterval(sampledTime);
            }
        }
    }

    [Serializable]
    public class Intervals
    {
        [SerializeField] private float steps;
        [SerializeField] private UnityEvent trigger;
        private int lastInterval;

        public float GetIntervalLength(float bpm)
        {
            return 60f / (bpm * steps);
        }

        public void CheckForNewInterval(float interval)
        {
            if (Mathf.FloorToInt(interval) != lastInterval)
            {
                lastInterval = Mathf.FloorToInt(interval);
                trigger?.Invoke();
            }
        }
    }
}
