using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioScripts
{
    public class AudioSourcePool : MonoBehaviour
    {
        [SerializeField] private int poolSize = 10;
        [SerializeField] private int maxSize = 25;
        [SerializeField] private List<GameObject> pool;

        private void Awake()
        {
            pool = new List<GameObject>();

            for (int i = 0; i < poolSize; i++)
            {
                var src = new GameObject("AudioSource_" + i);
                AudioSource newSource = src.AddComponent<AudioSource>();
                newSource.playOnAwake = false;
                pool.Add(newSource.gameObject);
                newSource.gameObject.SetActive(false);
            }
        }

        public AudioSource GetAvailableSource()
        {
            GameObject availableSource = pool.Find(s => s != null && !s.activeSelf);

            if (availableSource != null)
            {
                availableSource.SetActive(true);
                return availableSource.GetComponent<AudioSource>();
            }

            if (pool.Count < maxSize)
            {
                Debug.LogWarning("AudioSourcePool: No available sources found, creating a new one.");

                var src = new GameObject("AudioSource_" + pool.Count);
                var newAudioSource = src.AddComponent<AudioSource>();
                newAudioSource.playOnAwake = false;

                src.SetActive(true); // activate immediately
                pool.Add(src);

                return newAudioSource;
            }
            else
            {
                Debug.LogWarning("AudioSourcePool: Max pool size reached. Reusing first source.");
                availableSource = pool[0];
                availableSource.SetActive(true);
                return availableSource.GetComponent<AudioSource>();
            }
        }


        public void ReleaseSource(AudioSource source)
        {
            if (pool.Contains(source.gameObject))
            {
                source.Stop();
                source.clip = null;
                source.loop = false;
                source.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("AudioSourcePool: Tried to release source not in pool.");
            }
        }

        public void StopAll()
        {
            foreach (var src in pool)
            {
                src.GetComponent<AudioSource>().Stop();
                ReleaseSource(src.GetComponent<AudioSource>());
            }
        } 
        public bool IsAudible(AudioSource audioSource, Transform playerTransform, float hearingThreshold = 0.01f)
        {
            if (audioSource == null || playerTransform == null)
                return false;
       
            if (!audioSource.isPlaying)
                return false;
       
            float distance = Vector3.Distance(audioSource.transform.position, playerTransform.position);
       
            float volumeAtDistance = audioSource.volume;
       
            if (audioSource.spatialBlend > 0f)
            {
                // Apply 3D rolloff attenuation manually (approximate)
                float maxDistance = audioSource.maxDistance;
                float minDistance = audioSource.minDistance;
                float rolloffFactor = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
                volumeAtDistance *= 1f - rolloffFactor;
            }
       
            return volumeAtDistance > hearingThreshold;
        }
    }
}
