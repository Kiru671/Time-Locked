using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioScripts
{
    public class MusicPlaylist : MonoBehaviour
    {
        [SerializeField] public MusicTrack[] tracks;
        [HideInInspector] public MusicTrack currentTrack;
        [Tooltip("Leave empty on inspector.")]
        public MusicTrack[] queue;

        [Tooltip("Order is wholeTrack, percussion, bass, melody, other")]
        [SerializeField] private SourceParams[] musicParamaters = new SourceParams[5];
        [HideInInspector] public SourceParams[] MusicParamaters => musicParamaters;
        
        

        private void Start()
        {
            queue = tracks;
            AudioManager.Instance.SetMusicLayers(queue[0], musicParamaters);
        }

        public void NextTrack()
        {
            AudioManager.Instance.SetMusicLayers(queue[Mathf.Clamp(Array.IndexOf(queue, currentTrack) + 1, 0, 
                queue.Length - 1)], musicParamaters);
        }

        public void NewQueue(MusicTrack[] queueTracks)
        {
            queue = queueTracks;
            if (queue.Length > 0)
            {
                currentTrack = queue[0];
                AudioManager.Instance.SetMusicLayers(currentTrack, musicParamaters);
            }
        }
    }
    
    [Serializable]
    public struct MusicTrack
    {
        public string name;
        public AudioClip wholeTrack;
        public AudioClip percussion;
        public AudioClip bass;
        public AudioClip melody;
        public AudioClip harmony;
        public AudioClip other;
        [Range(0,100f)] public float volume;
        public bool utilizeWholeTrack;
        public float bpm;
    }
}
