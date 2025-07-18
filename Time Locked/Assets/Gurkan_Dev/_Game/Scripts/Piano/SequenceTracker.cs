using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SequenceTracker : MonoBehaviour
{
    [Header("Sequence Settings")]
    public string[] TargetSequence = {"C4", "D4", "E4", "F4"}; // Hedef sekans
    public float SequenceTimeout = 3f; // Sekans timeout süresi (saniye)
    public bool ResetOnWrongNote = true; // Yanlış nota çalınırsa sıfırla
    
    [Header("Debug")]
    public bool ShowDebugLogs = true;
    
    private List<string> currentSequence = new List<string>();
    private float lastNoteTime;
    
    // Singleton pattern için
    public static SequenceTracker Instance { get; private set; }
    
    void Awake()
    {
        // Singleton kontrolü
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (ShowDebugLogs)
        {
            Debug.Log($"SequenceTracker başlatıldı. Hedef sekans: {string.Join(" -> ", TargetSequence)}");
        }
    }
    
    void Update()
    {
        // Timeout kontrolü
        if (currentSequence.Count > 0 && Time.time - lastNoteTime > SequenceTimeout)
        {
            if (ShowDebugLogs)
            {
                Debug.Log("Sekans timeout oldu, sıfırlanıyor...");
            }
            ResetSequence();
        }
    }
    
    public void OnNotePressed(string noteName)
    {
        lastNoteTime = Time.time;
        
        if (ShowDebugLogs)
        {
            Debug.Log($"Nota çalındı: {noteName}");
        }
        
        // Eğer bu sekansın ilk notası ise
        if (currentSequence.Count == 0)
        {
            if (noteName == TargetSequence[0])
            {
                currentSequence.Add(noteName);
                if (ShowDebugLogs)
                {
                    Debug.Log($"Sekans başladı! ({currentSequence.Count}/{TargetSequence.Length})");
                }
            }
            return;
        }
        
        // Beklenen nota mı?
        if (currentSequence.Count < TargetSequence.Length && 
            noteName == TargetSequence[currentSequence.Count])
        {
            currentSequence.Add(noteName);
            
            if (ShowDebugLogs)
            {
                Debug.Log($"Doğru nota! İlerleme: ({currentSequence.Count}/{TargetSequence.Length})");
            }
            
            // Sekans tamamlandı mı?
            if (currentSequence.Count == TargetSequence.Length)
            {
                OnSequenceCompleted();
            }
        }
        else
        {
            // Yanlış nota
            if (ShowDebugLogs)
            {
                Debug.Log($"Yanlış nota! Beklenen: {TargetSequence[currentSequence.Count]}, Çalınan: {noteName}");
            }
            
            if (ResetOnWrongNote)
            {
                ResetSequence();
                
                // Eğer yanlış nota sekansın ilk notası ise, yeniden başlat
                if (noteName == TargetSequence[0])
                {
                    currentSequence.Add(noteName);
                    if (ShowDebugLogs)
                    {
                        Debug.Log($"Sekans yeniden başladı! ({currentSequence.Count}/{TargetSequence.Length})");
                    }
                }
            }
        }
    }
    
    private void OnSequenceCompleted()
    {
        Debug.Log("🎉 TEBRİKLER! İSTENEN NOTA SEKANSI TAMAMLANDI! 🎉");
        Debug.Log($"Tamamlanan sekans: {string.Join(" -> ", currentSequence)}");
        
        // Burada istediğiniz ek işlemleri yapabilirsiniz
        // Örneğin: ses efekti çalma, UI güncellemesi, vb.
        
        ResetSequence();
    }
    
    private void ResetSequence()
    {
        currentSequence.Clear();
    }
    
    // Public metodlar
    public void SetTargetSequence(string[] newSequence)
    {
        TargetSequence = newSequence;
        ResetSequence();
        
        if (ShowDebugLogs)
        {
            Debug.Log($"Yeni hedef sekans ayarlandı: {string.Join(" -> ", TargetSequence)}");
        }
    }
    
    public void AddCustomSequence(string sequenceName, string[] sequence)
    {
        if (ShowDebugLogs)
        {
            Debug.Log($"Özel sekans eklendi - {sequenceName}: {string.Join(" -> ", sequence)}");
        }
        // Burada multiple sequences için dictionary kullanabilirsiniz
    }
    
    // Mevcut ilerlemeyi göster
    public void ShowCurrentProgress()
    {
        if (currentSequence.Count > 0)
        {
            Debug.Log($"Mevcut ilerleme: {string.Join(" -> ", currentSequence)} ({currentSequence.Count}/{TargetSequence.Length})");
        }
        else
        {
            Debug.Log("Henüz sekans başlatılmadı.");
        }
    }
} 