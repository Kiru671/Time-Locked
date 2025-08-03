using UnityEngine;
using System.Collections;
using Sample;

public class GhostGuideSimple : MonoBehaviour
{
    [Header("Ghost Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 2f;
    
    [Header("Player Guidance")]
    public float guidanceDistance = 8f; // Oyuncuya ne kadar yaklaşacak
    public float teleportDistance = 3f; // Oyuncuya ne kadar yaklaşınca teleport edecek
    public float teleportCooldown = 5f; // Teleport sonrası bekleme süresi
    
    [Header("Visual Effects")]
    public GameObject spawnEffect;
    public GameObject teleportEffect;
    public Light ghostLight;
    public Color ghostColor = Color.cyan;
    
    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip teleportSound;
    public AudioClip guidanceSound;
    
    private Transform[] puzzlePoints;
    private Transform playerTransform;
    private Transform currentTarget;
    private AudioSource audioSource;
    private Animator animator;
    
    private int currentPuzzleIndex = 0;
    private bool isGuiding = false;
    private bool canTeleport = true;
    private float lastTeleportTime;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Mevcut GhostScript'i devre dışı bırak
        var existingGhostScript = GetComponent<Sample.GhostScript>();
        if (existingGhostScript != null)
        {
            existingGhostScript.enabled = false;
        }
        
        // CharacterController'ı devre dışı bırak
        var characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }
    
    public void Initialize(Transform[] points, Transform player)
    {
        puzzlePoints = points;
        playerTransform = player;
        currentPuzzleIndex = 0;
        
        // Spawn efektleri
        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, transform.position, Quaternion.identity);
        }
        
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
        
        // Hayalet ışığını ayarla
        if (ghostLight != null)
        {
            ghostLight.color = ghostColor;
        }
        
        // İlk hedefi belirle
        SetNextTarget();
        
        // Rehberlik modunu başlat
        StartCoroutine(GuidePlayer());
    }
    
    private void SetNextTarget()
    {
        if (puzzlePoints != null && currentPuzzleIndex < puzzlePoints.Length)
        {
            currentTarget = puzzlePoints[currentPuzzleIndex];
            
            if (currentTarget != null)
            {
                isGuiding = true;
                
                if (showDebugInfo)
                    Debug.Log($"Hayalet hedef: {currentTarget.name}");
            }
        }
    }
    
    private IEnumerator GuidePlayer()
    {
        while (isGuiding && currentTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            // Oyuncuya yaklaş ve rehberlik et
            if (distanceToPlayer > guidanceDistance)
            {
                // Oyuncuya yaklaş
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                Vector3 targetPosition = playerTransform.position - (directionToPlayer * guidanceDistance);
                MoveTowards(targetPosition);
            }
            else
            {
                // Hedef noktaya git
                MoveTowards(currentTarget.position);
            }
            
            // Oyuncu hedefe yaklaştıysa teleport et
            if (distanceToTarget < teleportDistance && canTeleport)
            {
                TeleportPlayer();
            }
            
            // Animasyon kontrolü
            if (animator != null)
            {
                float speed = GetComponent<Rigidbody>()?.linearVelocity.magnitude ?? 0f;
                if (speed > 0.1f)
                {
                    // Hareket animasyonu
                    animator.CrossFade("Base Layer.move", 0.1f, 0, 0);
                }
                else
                {
                    // Idle animasyonu
                    animator.CrossFade("Base Layer.idle", 0.1f, 0, 0);
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Y eksenini sıfırla
        
        if (direction.magnitude > 0.1f)
        {
            // Hareket
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // Rotasyon
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void TeleportPlayer()
    {
        if (!canTeleport) return;
        
        // Teleport efektleri
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, playerTransform.position, Quaternion.identity);
        }
        
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
        
        // Oyuncuyu hedef noktaya teleport et
        Vector3 teleportPosition = currentTarget.position + (currentTarget.forward * 2f);
        playerTransform.position = teleportPosition;
        
        // Oyuncunun bakış açısını hedefe çevir
        Vector3 lookDirection = (currentTarget.position - teleportPosition).normalized;
        if (lookDirection != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.LookRotation(lookDirection);
        }
        
        // Teleport cooldown
        canTeleport = false;
        lastTeleportTime = Time.time;
        StartCoroutine(TeleportCooldown());
        
        // Sonraki puzzle noktasına geç
        currentPuzzleIndex++;
        if (currentPuzzleIndex < puzzlePoints.Length)
        {
            SetNextTarget();
        }
        else
        {
            // Tüm puzzle noktaları tamamlandı
            OnGuidanceComplete();
        }
        
        if (showDebugInfo)
            Debug.Log($"Oyuncu teleport edildi: {currentTarget.name}");
    }
    
    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
    
    private void OnGuidanceComplete()
    {
        isGuiding = false;
        
        if (showDebugInfo)
            Debug.Log("Hayalet rehberliği tamamlandı!");
        
        // Tamamlanma efektleri
        StartCoroutine(FadeOutAndDestroy());
    }
    
    private IEnumerator FadeOutAndDestroy()
    {
        // Fade out efekti
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float fadeTime = 2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - (elapsedTime / fadeTime);
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color color = renderer.material.color;
                    color.a = alpha;
                    renderer.material.color = color;
                }
            }
            
            if (ghostLight != null)
            {
                ghostLight.intensity = alpha;
            }
            
            yield return null;
        }
        
        // Hayaleti yok et
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 1f);
        }
        
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, guidanceDistance);
            Gizmos.DrawWireSphere(playerTransform.position, teleportDistance);
        }
    }
} 