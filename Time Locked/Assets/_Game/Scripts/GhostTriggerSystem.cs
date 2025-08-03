using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostTriggerSystem : MonoBehaviour
{
    [Header("Trigger Settings")]
    public float triggerTime = 90f; // 90 saniye
    public float ghostWaitTime = 7f; // Ghost'un waypoint'te bekleme süresi
    public float spawnDistanceFromPlayer = 5f; // Oyuncuya yakın spawn mesafesi
    
    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    public Transform[] waypoints; // Puzzle waypoint'leri
    public Transform playerTransform;
    
    [Header("Item Classes")]
    public string[] triggerItemClasses; // Tetikleyici item sınıfları
    
    [Header("Visual Effects")]
    public GameObject ghostSpawnEffect;
    public AudioClip ghostSpawnSound;
    
    private PlayerInventory playerInventory;
    private PuzzleTimerManager puzzleTimer;
    private AudioSource audioSource;
    
    private bool isGhostActive = false;
    private GameObject currentGhost;
    private Coroutine triggerCoroutine;
    private Dictionary<string, float> itemHoldTimes = new Dictionary<string, float>();
    
    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        puzzleTimer = FindFirstObjectByType<PuzzleTimerManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Item hold time'larını başlat
        foreach (string itemClass in triggerItemClasses)
        {
            itemHoldTimes[itemClass] = 0f;
        }
    }
    
    private void Update()
    {
        if (isGhostActive) return;
        
        CheckItemHoldTimes();
    }
    
    private void CheckItemHoldTimes()
    {
        if (playerInventory == null) return;
        
        foreach (string itemClass in triggerItemClasses)
        {
            if (playerInventory.HasItemOfClass(itemClass))
            {
                // Item tutuluyor, süreyi artır
                itemHoldTimes[itemClass] += Time.deltaTime;
                
                // Puzzle'da ilerleme var mı kontrol et
                if (puzzleTimer != null && !puzzleTimer.HasRecentProgress())
                {
                    // 90 saniye geçti mi kontrol et
                    if (itemHoldTimes[itemClass] >= triggerTime)
                    {
                        TriggerGhost(itemClass);
                        return;
                    }
                }
                else
                {
                    // Puzzle'da ilerleme var, süreyi sıfırla
                    itemHoldTimes[itemClass] = 0f;
                }
            }
            else
            {
                // Item tutulmuyor, süreyi sıfırla
                itemHoldTimes[itemClass] = 0f;
            }
        }
    }
    
    private void TriggerGhost(string itemClass)
    {
        if (isGhostActive) return;
        
        Debug.Log($"Ghost tetiklendi! Item: {itemClass}");
        
        // Ghost'u spawn et
        SpawnGhost();
        
        // Tetikleyici coroutine'i başlat
        triggerCoroutine = StartCoroutine(GhostCycle(itemClass));
    }
    
    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerTransform == null) return;
        
        // Oyuncuya yakın bir pozisyon bul
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Ghost'u spawn et
        currentGhost = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
        
        // Ghost script'ini ayarla
        var ghostGuide = currentGhost.GetComponent<GhostGuideSimple>();
        if (ghostGuide != null)
        {
            ghostGuide.Initialize(waypoints, playerTransform);
        }
        
        // Spawn efektleri
        if (ghostSpawnEffect != null)
        {
            Instantiate(ghostSpawnEffect, spawnPosition, Quaternion.identity);
        }
        
        if (ghostSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ghostSpawnSound);
        }
        
        isGhostActive = true;
    }
    
    private Vector3 GetSpawnPosition()
    {
        if (playerTransform == null) return Vector3.zero;
        
        // Oyuncunun etrafında rastgele bir pozisyon bul
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0; // Y eksenini sıfırla
        
        Vector3 spawnPosition = playerTransform.position + (randomDirection * spawnDistanceFromPlayer);
        
        // NavMesh üzerinde geçerli bir pozisyon bul
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return spawnPosition;
    }
    
    private IEnumerator GhostCycle(string itemClass)
    {
        while (true)
        {
            // Ghost'un waypoint'lere gitmesini bekle
            yield return new WaitForSeconds(ghostWaitTime);
            
            // Ghost'u yok et
            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }
            
            isGhostActive = false;
            
            // Item hala tutuluyor mu ve puzzle'da ilerleme yok mu kontrol et
            if (playerInventory.HasItemOfClass(itemClass) && 
                (puzzleTimer == null || !puzzleTimer.HasRecentProgress()))
            {
                // Yeniden başlat
                yield return new WaitForSeconds(2f); // Kısa bir bekleme
                TriggerGhost(itemClass);
            }
            else
            {
                // Döngüyü sonlandır
                break;
            }
        }
    }
    
    public void ResetTrigger(string itemClass)
    {
        if (itemHoldTimes.ContainsKey(itemClass))
        {
            itemHoldTimes[itemClass] = 0f;
        }
    }
    
    public void ForceStopGhost()
    {
        if (triggerCoroutine != null)
        {
            StopCoroutine(triggerCoroutine);
            triggerCoroutine = null;
        }
        
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
        
        isGhostActive = false;
        
        // Tüm item sürelerini sıfırla
        foreach (string itemClass in triggerItemClasses)
        {
            itemHoldTimes[itemClass] = 0f;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnDistanceFromPlayer);
        }
        
        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
    }
} 