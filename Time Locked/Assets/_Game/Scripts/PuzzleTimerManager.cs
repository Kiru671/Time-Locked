using UnityEngine;
using System.Collections;
using StarterAssets;

public class PuzzleTimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float inactivityThreshold = 90f; // 90 saniye
    public float checkInterval = 1f; // Her saniye kontrol et
    
    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    public Transform[] puzzlePoints; // Puzzle noktaları
    public float ghostSpawnDistance = 5f; // Oyuncudan ne kadar uzakta spawn olsun
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private float lastInteractionTime;
    private float lastProgressTime; // Son ilerleme zamanı
    private bool ghostSpawned = false;
    private bool isTimerActive = false;
    private Transform playerTransform;
    private GhostGuideSimple currentGhost;
    
    public static PuzzleTimerManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            playerTransform = FindFirstObjectByType<FirstPersonController>()?.transform;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("PuzzleTimerManager: Player not found!");
            return;
        }
        
        // Timer'ı başlat
        StartTimer();
    }
    
    public void StartTimer()
    {
        if (!isTimerActive)
        {
            isTimerActive = true;
            lastInteractionTime = Time.time;
            lastProgressTime = Time.time; // İlk başlangıçta ilerleme zamanını ayarla
            StartCoroutine(CheckInactivityTimer());
            
            if (showDebugInfo)
                Debug.Log("Puzzle Timer başlatıldı. 90 saniye sonra hayalet spawn olacak.");
        }
    }
    
    public void ResetTimer()
    {
        lastInteractionTime = Time.time;
        lastProgressTime = Time.time; // İlerleme zamanını da güncelle
        ghostSpawned = false;
        
        if (currentGhost != null)
        {
            Destroy(currentGhost.gameObject);
            currentGhost = null;
        }
        
        if (showDebugInfo)
            Debug.Log("Puzzle Timer sıfırlandı.");
    }
    
    public void OnPuzzleInteraction()
    {
        ResetTimer();
    }
    
    // Son ilerleme zamanını güncelle
    public void OnPuzzleProgress()
    {
        lastProgressTime = Time.time;
        if (showDebugInfo)
            Debug.Log("Puzzle ilerlemesi kaydedildi.");
    }
    
    // Son ilerleme zamanını kontrol et
    public bool HasRecentProgress()
    {
        return (Time.time - lastProgressTime) < 30f; // Son 30 saniye içinde ilerleme var mı?
    }
    
    private IEnumerator CheckInactivityTimer()
    {
        while (isTimerActive)
        {
            yield return new WaitForSeconds(checkInterval);
            
            if (Time.time - lastInteractionTime >= inactivityThreshold && !ghostSpawned)
            {
                SpawnGhost();
            }
        }
    }
    
    private void SpawnGhost()
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("PuzzleTimerManager: Ghost prefab atanmamış!");
            return;
        }
        
        if (puzzlePoints == null || puzzlePoints.Length == 0)
        {
            Debug.LogError("PuzzleTimerManager: Puzzle noktaları atanmamış!");
            return;
        }
        
        // Oyuncunun yakınında rastgele bir nokta seç
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Hayaleti spawn et
        GameObject ghostObject = Instantiate(ghostPrefab, spawnPosition, Quaternion.identity);
        currentGhost = ghostObject.GetComponent<GhostGuideSimple>();
        
        if (currentGhost == null)
        {
            currentGhost = ghostObject.AddComponent<GhostGuideSimple>();
        }
        
        // Hayalete puzzle noktalarını ver
        currentGhost.Initialize(puzzlePoints, playerTransform);
        
        ghostSpawned = true;
        
        if (showDebugInfo)
            Debug.Log("Hayalet spawn edildi! Oyuncuyu bir sonraki puzzle noktasına götürecek.");
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0; // Y eksenini sıfırla (yerde spawn olsun)
        
        Vector3 spawnPos = playerPos + (randomDirection * ghostSpawnDistance);
        
        // NavMesh üzerinde geçerli bir nokta bul
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return spawnPos;
    }
    
    private void OnGUI()
    {
        if (showDebugInfo)
        {
            float remainingTime = inactivityThreshold - (Time.time - lastInteractionTime);
            if (remainingTime > 0)
            {
                GUI.Label(new Rect(10, 10, 300, 20), $"Hayalet Spawn Süresi: {remainingTime:F1} saniye");
            }
            else
            {
                GUI.Label(new Rect(10, 10, 300, 20), "Hayalet aktif!");
            }
        }
    }
} 