using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace AudioScripts
{
    [RequireComponent(typeof(AudioSourcePool), typeof(MusicLayerController))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSourcePool audioSourcePool;
        [SerializeField] private MusicLayerController musicLayerController;
        [SerializeField] private BeatManager beatManager;
        [SerializeField] private SourceParams defaultSfxParams;
        [SerializeField] private SourceParams[] defaultMusicParams;
        [SerializeField] private List<AudioSource> activeSources;
        private AmbianceVolumeManager ambianceVolumeManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            activeSources = new List<AudioSource>();
            //ambianceVolumeManager = gameObject.AddComponent<AmbianceVolumeManager>();
        }

        public AudioSource PlaySfx(SoundData sdata, Vector3 position, bool loop = false, float pitch = 1f, float volume = 1f)
        {
            var source = audioSourcePool.GetAvailableSource();
            sdata.sourceParams?.ApplyTo(source);
            source.transform.position = position;
            source.clip = sdata.clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.Play();
            if (!activeSources.Contains(source))
                activeSources.Add(source);
            StartCoroutine(ReleaseSourceAfterPlay(source));
            return source;
        }
        
        public void SetMusicLayers(MusicTrack track, SourceParams[] layerParams)
        {
            beatManager.BPM = track.bpm;
            beatManager.audioSource = musicLayerController.MusicLayers[(int)MusicLayerController.LayerType.WholeTrack];

            Debug.Log("AudioManager: Set music layers with BPM: " + track.bpm);

            var clips = new AudioClip[]
                { track.wholeTrack, track.percussion, track.bass, track.melody, track.harmony, track.other };
            
            // Assign clips to layers, skip whole track layer if not needed. Can be re-enabled through code.
            
            for (int i = track.utilizeWholeTrack ? 0:1; i < clips.Length; i++)
            {
                if (i < musicLayerController.MusicLayers.Count)
                {
                    var source = musicLayerController.MusicLayers[i];
                    source.clip = clips[i];
                    musicLayerController.SetLayerActive(i, source.clip != null);

                    if (layerParams != null && i < layerParams.Length && layerParams[i] != null)
                    {
                        layerParams[i].ApplyTo(source);
                    }
                }
            }
        }

        public void SetNextTrack(MusicTrack track)
        {
            SetMusicLayers(track, defaultMusicParams);
        }
        
        public void SetMusicQueue(MusicTrack[] queue)
        {
            if (queue.Length == 0) return;
            MusicPlaylist playlist = FindObjectOfType<MusicPlaylist>();
            if (playlist == null)
            {
                Debug.LogWarning("AudioManager: No MusicPlaylist found in the scene.");
                return;
            }
            playlist.NewQueue(queue);
            SetMusicLayers(queue[0], defaultMusicParams);
        }

        public void SkipTrack()
        {
            MusicPlaylist playlist = GameObject.FindObjectOfType<MusicPlaylist>();
            if (playlist == null)
                return;
            SetMusicLayers(playlist.queue[Array.IndexOf(playlist.queue, playlist.currentTrack) + 1],
                defaultMusicParams);
        }

        public void PlayRandom(SoundData[] sdata, Vector3 position)
        {
            if (sdata.Length == 0) return;
            var randomIndex = Random.Range(0, sdata.Length);
            PlaySfx(sdata[randomIndex], position);
        }

        public void SetMusicLayerActive(int index, bool active)
        {
            musicLayerController.SetLayerActive(index, active);
        }

        public void StopAllSfx()
        {
            audioSourcePool.StopAll();
        }

        public void StopWithFade(AudioSource source, float fadeTime = 1f)
        {
            StartCoroutine(FadeOutAndStop(source, fadeTime));
        }

        private IEnumerator FadeOutAndStop(AudioSource source, float fadeTime)
        {
            float startVolume = source.volume;
            float t = 0;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }

            source.Stop();
            source.loop = false;
            audioSourcePool.ReleaseSource(source);
        }

        public IEnumerator SourceFollow(AudioSource source, Func<Vector3> getTargetPosition)
        {
            while (source.isPlaying)
            {
                source.transform.position = getTargetPosition();
                yield return null;
            }
        }

        private IEnumerator MonitorSources()
        {
            var wait = new WaitForSeconds(1f); // Check once per second
            while (true)
            {
                for (int i = activeSources.Count - 1; i >= 0; i--)
                {
                    var src = activeSources[i];
                    
                    if ((!src.loop && !src.isPlaying) ||
                        audioSourcePool.IsAudible(src, Camera.main?.transform, 0.01f) == false)
                    {
                        {
                            activeSources.RemoveAt(i);
                            audioSourcePool.ReleaseSource(src);
                        }
                    }
                }
                yield return wait;
            }
        }
        private IEnumerator ReleaseSourceAfterPlay(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            source.clip = null;
            source.gameObject.SetActive(false);
        }

        public void ReleaseSource(AudioSource src)
        {
            audioSourcePool.ReleaseSource(src);
        }
        
    }
    [Serializable]
    public struct SoundData
    {
        public AudioClip clip;
        public SourceParams sourceParams;
    }
}