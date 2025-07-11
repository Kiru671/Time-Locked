using UnityEngine;

namespace AudioScripts
{
    public class MusicSetterVolume : MonoBehaviour
    {
        [SerializeField] private MusicTrack[] tracks;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                AudioManager.Instance.SetMusicQueue(tracks);
            }
        }
    }
}
