using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AudioScripts
{
    public class AmbianceVolume : MonoBehaviour
    {
        [HideInInspector] public AudioSource source;
        [HideInInspector] public AmbianceVolume currentVolume;
        [HideInInspector] public AmbianceVolume blockingVolume;
        [HideInInspector] public BoxCollider[] colliderVolumes;
        private GameObject player;
        private Collider colliderVolume;
        private AudioManager instance;
        
        public SoundData sdata;
        
        public event Action<AmbianceVolume> OnEnterVolume;
        public event Action<AmbianceVolume> OnLeaveVolume;

        private void Start()
        {
            blockingVolume = this;
        }

        private void OnEnable()
        {
            colliderVolume = GetComponent<Collider>();
            colliderVolumes = GetComponents<BoxCollider>();
            WaitForPlayerAndPlay();
            instance = AudioManager.Instance;
        }
        
        private async void WaitForPlayerAndPlay()
        {
            // Wait until a GameObject with the tag "Player" exists
            while (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                await System.Threading.Tasks.Task.Yield(); // Wait for the next frame
            }

            source = instance.PlaySfx(sdata, transform.position, true);
            StartCoroutine(instance.SourceFollow(source, () =>
            {
                var playerPos = player.transform.position;

                if (currentVolume != this)
                {
                    return GetClosestSurfacePointFromColliders(playerPos, blockingVolume.colliderVolumes);
                }
                    
                return GetClosestPointInCollider(playerPos);
            }));
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnEnterVolume?.Invoke(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnLeaveVolume?.Invoke(this);
            }
        }
        
        private Vector3 GetClosestPointInCollider(Vector3 pos)
        {
            var closestPoint = colliderVolume.ClosestPoint(pos);
            return new Vector3(closestPoint.x, closestPoint.y, closestPoint.z);
        }
        
        private Vector3 GetClosestSurfacePoint(Bounds bounds, Vector3 point)
        {
            Vector3 localPoint = point - bounds.center;
            Vector3 halfSize = bounds.extents;
        
            // Clamp Y only inside the volume, no snapping to top/bottom surface
            localPoint.y = Mathf.Clamp(localPoint.y, -halfSize.y, halfSize.y);
        
            // For X and Z, clamp inside the box bounds first
            localPoint.x = Mathf.Clamp(localPoint.x, -halfSize.x, halfSize.x);
            localPoint.z = Mathf.Clamp(localPoint.z, -halfSize.z, halfSize.z);
        
            // Calculate distances to the vertical faces (X and Z only)
            float distToLeft = Mathf.Abs(localPoint.x + halfSize.x);
            float distToRight = Mathf.Abs(localPoint.x - halfSize.x);
            float distToFront = Mathf.Abs(localPoint.z + halfSize.z);
            float distToBack = Mathf.Abs(localPoint.z - halfSize.z);
        
            // Find minimum distance face on X or Z axis
            float minDistX = Mathf.Min(distToLeft, distToRight);
            float minDistZ = Mathf.Min(distToFront, distToBack);
        
            if (minDistX < minDistZ)
            {
                // Snap X to nearest vertical face
                localPoint.x = (distToLeft < distToRight) ? -halfSize.x : halfSize.x;
                // Z stays as is (clamped inside)
            }
            else
            {
                // Snap Z to nearest vertical face
                localPoint.z = (distToFront < distToBack) ? -halfSize.z : halfSize.z;
                // X stays as is (clamped inside)
            }
        
            return bounds.center + localPoint;
        }
        public Vector3 GetClosestSurfacePointFromColliders(Vector3 playerPos, BoxCollider[] colliders)
        {
            float closestSqrDist = float.MaxValue;
            Vector3 closestSurface = Vector3.zero;

            foreach (var box in colliders)
            {
                Vector3 surface = GetClosestSurfacePoint(box.bounds, playerPos);
                float dist = (surface - playerPos).sqrMagnitude;

                if (dist < closestSqrDist)
                {
                    closestSqrDist = dist;
                    closestSurface = surface;
                }
            }

            return closestSurface;
        }
    }
}
