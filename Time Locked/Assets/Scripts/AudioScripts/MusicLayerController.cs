using System.Collections.Generic;
using UnityEngine;

namespace AudioScripts
{
    public class MusicLayerController : MonoBehaviour
    {
        private int layerCount = System.Enum.GetNames(typeof(LayerType)).Length;
        private List<AudioSource> musicLayers;
        public List<AudioSource> MusicLayers { get { return musicLayers; } }

        private void Awake()
        {
            musicLayers = new List<AudioSource>();

            for (int i = 0; i < layerCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = true;
                src.volume = 0f; // Start all layers muted or silent
                musicLayers.Add(src);
            }
        }

        
        /// Layer 1 is percussion, Layer 2 is bass, Layer 3 is melody, Layer 4 is harmony.
        public void SetLayerActive(int index, bool active)
        {
            if (index < 0 || index >= musicLayers.Count) return;
            musicLayers[index].volume = active ? 1f : 0f;
            musicLayers[index].Play();
        }

        public void Stop()
        {
            foreach (var layer in musicLayers)
            {
                layer.Stop();
            }
        }
        
        public enum LayerType
        {
            WholeTrack,
            Percussion,
            Bass,
            Melody,
            Harmony,
            Other
        }
    }
}