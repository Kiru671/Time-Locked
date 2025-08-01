using UnityEngine;

namespace AudioScripts
{
    [CreateAssetMenu(fileName = "New Source Parameters", menuName = "Audio/Source Parameters")]
    public class SourceParams : ScriptableObject
    {
        [Header("Audio Source Settings")]
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(-3f, 3f)]
        public float pitch = 1f;
        public bool looping = false;
        [Range(0f, 1f)] 
        public float spatialBlend = 0f;
        public float dopplerLevel = 1f;
        public float maxDistance = 500f;
        public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup;
        public AudioRolloffMode outputAudioRolloffMode;
        public AnimationCurve customRolloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public bool playOnAwake = false;
        
        public void ApplyTo(AudioSource source)
        {
            source.volume = volume;
            source.pitch = pitch;
            source.loop = looping;
            source.spatialBlend = spatialBlend;
            source.dopplerLevel = dopplerLevel;
            source.maxDistance = maxDistance;
            source.outputAudioMixerGroup = outputAudioMixerGroup;
            source.rolloffMode = outputAudioRolloffMode;
            source.playOnAwake = playOnAwake;
            if(source.rolloffMode == AudioRolloffMode.Custom)
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloffCurve);
        }
    }
}
