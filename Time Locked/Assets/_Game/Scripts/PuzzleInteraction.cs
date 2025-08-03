using UnityEngine;

public class PuzzleInteraction : MonoBehaviour, IInteractable
{
    [Header("Puzzle Settings")]
    public string puzzleName = "Puzzle Item";
    public string interactionText = "Press E to interact";
    public bool isCompleted = false;
    
    [Header("Visual Feedback")]
    public GameObject completionEffect;
    public Material completedMaterial;
    public Light puzzleLight;
    public Color completedColor = Color.green;
    
    [Header("Audio")]
    public AudioClip interactionSound;
    public AudioClip completionSound;
    
    private Renderer objectRenderer;
    private AudioSource audioSource;
    private Material originalMaterial;
    
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }
    
    public string GetInteractionText()
    {
        if (isCompleted)
        {
            return $"{puzzleName} - Completed";
        }
        return $"{interactionText} {puzzleName}";
    }
    
    public void Interact(PlayerInventory player)
    {
        if (isCompleted) return;
        
        // Etkileşim sesi
        if (interactionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
        
        // Puzzle timer'ı sıfırla
        if (PuzzleTimerManager.Instance != null)
        {
            PuzzleTimerManager.Instance.OnPuzzleInteraction();
        }
        
        // UIManager null check
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideHint();
        }
        
        // Puzzle'ı tamamla
        CompletePuzzle();
    }
    
    private void CompletePuzzle()
    {
        isCompleted = true;
        
        // Tamamlanma efektleri
        if (completionEffect != null)
        {
            Instantiate(completionEffect, transform.position, Quaternion.identity);
        }
        
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Materyal değiştir
        if (objectRenderer != null && completedMaterial != null)
        {
            objectRenderer.material = completedMaterial;
        }
        
        // Işık rengini değiştir
        if (puzzleLight != null)
        {
            puzzleLight.color = completedColor;
        }
        
        // Puzzle ilerlemesini kaydet (Ghost tetikleyici için)
        if (PuzzleTimerManager.Instance != null)
        {
            PuzzleTimerManager.Instance.OnPuzzleProgress();
        }
        
        Debug.Log($"{puzzleName} tamamlandı!");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
} 