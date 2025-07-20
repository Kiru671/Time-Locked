using System.Collections.Generic;
using UnityEngine;

namespace AudioScripts
{
    public class AmbianceVolumeManager : MonoBehaviour
    {
        private List<AmbianceVolume> ambianceVolumes = new List<AmbianceVolume>();
        private List<AmbianceVolume> depthList = new List<AmbianceVolume>();
        private GameObject player;
        private BoxCollider[] allColliders;
    
        private void OnEnable()
        {
            WaitForPlayer();
        
            foreach (var volume in ambianceVolumes)
            {
                allColliders = volume.GetComponents<BoxCollider>();
                foreach(var col in allColliders)
                {
                    if (col.bounds.Contains(player.transform.position))
                    {
                        if (!depthList.Contains(volume))
                        {
                            depthList.Add(volume);
                            UpdateVolumes(volume);
                        }
                    }
                }
            }
        
            foreach (var volume in FindObjectsOfType<AmbianceVolume>())
            {
                ambianceVolumes.Add(volume);
                volume.OnEnterVolume += (v) => 
                {
                    if (!depthList.Contains(v))
                    {
                        depthList.Add(v);
                        UpdateVolumes(depthList[^1]);
                    }
                };
                volume.OnLeaveVolume += (v) => 
                {
                    if (depthList.Contains(v))
                    {
                        depthList.Remove(v);
                        UpdateVolumes(depthList.Count > 0 ? depthList[^1] : null);
                    }
                };
            }
        }

        private void Update()
        {
            if (player == null) return;

            foreach (var volume in ambianceVolumes)
            {
                if (volume.source == null) continue;

                float distance = Vector3.Distance(
                    player.transform.position,
                    volume.GetClosestSurfacePointFromColliders(player.transform.position, volume.colliderVolumes)
                );

                if (distance <= volume.source.maxDistance)
                {
                    volume.enabled = true;
                }
                else if (Vector3.Distance(player.transform.position, volume.source.transform.position) > volume.source.maxDistance)
                {
                    volume.enabled = false;
                    AudioManager.Instance.ReleaseSource(volume.source);
                }
            }
        }
    
        private async void WaitForPlayer()
        {
            // Wait until a GameObject with the tag "Player" exists
            while (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                await System.Threading.Tasks.Task.Yield(); // Wait for the next frame
            }
        }

        private void UpdateVolumes(AmbianceVolume vol)
        {
            foreach (var volume in ambianceVolumes)
            {
                volume.currentVolume = vol;
            }
            
            if (depthList.Count > 0)
            {
                for (var i = depthList.Count - 1; i >= 0; i--)
                {
                    depthList[i].blockingVolume = depthList[i + depthList.Count == 1 ? 0 : 1];
                }
            }
        }
    }
}
